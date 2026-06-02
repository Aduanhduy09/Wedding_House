using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wedding_House.Models;
using MimeKit;
using MailKit.Net.Smtp;

namespace Wedding_House.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly WeddingHouseContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(WeddingHouseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ==========================================
        // 1. HÀM TRỢ GIÚP: GỬI EMAIL QUA CỔNG GOOGLE
        // ==========================================
        private void SendEmailOTP(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            // ĐÃ SỬA: Đồng bộ email người gửi trùng với tài khoản authenticate bên dưới
            message.From.Add(new MailboxAddress("Nhà hàng Wedding House", "weddinghouseadmin@gmail.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Mã OTP Xác Thực Tài Khoản Wedding House";

            message.Body = new TextPart("html")
            {
                Text = $"<h3>Cảm ơn bạn đã đăng ký!</h3>" +
                       $"<p>Mã OTP kích hoạt tài khoản của bạn là: <b style='color:red; font-size:20px;'>{otpCode}</b></p>" +
                       $"<p>Mã này có hiệu lực trong vòng 5 phút.</p>"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate("weddinghouseadmin@gmail.com", "pwof ydld xqgf jehc");
                client.Send(message);
                client.Disconnect(true);
            }
        }

        // ==========================================
        // 2. API ĐĂNG KÝ (ĐÃ TÍCH HỢP XOÁ TÀI KHOẢN RÁC)
        // ==========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // 🌟 RÀO CHẮN 1: Kiểm tra xem Email này đã có ai dùng chưa
            var isEmailExist = await _context.Khachhangs.AnyAsync(k => k.Email == model.Email);
            if (isEmailExist)
            {
                return BadRequest(new { message = "Email này đã được sử dụng bởi một tài khoản khác!" });
            }

            // 🌟 RÀO CHẮN 2: Kiểm tra xem Số điện thoại này đã có ai dùng chưa
            var isPhoneExist = await _context.Khachhangs.AnyAsync(k => k.DienThoai == model.DienThoai);
            if (isPhoneExist)
            {
                return BadRequest(new { message = "Số điện thoại này đã được đăng ký hệ thống!" });
            }
            // Tìm kiếm tài khoản xem đã tồn tại tên đăng nhập này chưa
            var existingUser = await _context.Taikhoans.FirstOrDefaultAsync(u => u.TenDangNhap == model.Username);

            if (existingUser != null)
            {
                // Trường hợp 1: Tài khoản đã kích hoạt (IsConfirmed = 1) hoặc dạng đặc biệt (NULL) -> Chặn luôn
                if (existingUser.IsConfirmed == true || existingUser.IsConfirmed == null)
                {
                    return BadRequest(new { message = "Tài khoản này đã tồn tại và đã được xác thực trong hệ thống!" });
                }
                // Trường hợp 2: Tài khoản đã có nhưng CHƯA kích hoạt (IsConfirmed = 0) -> Tiến hành dọn dẹp rác
                else
                {
                    // 1. Tìm và xóa Khách hàng liên quan trước để tránh lỗi ràng buộc khóa ngoại (FK)
                    var existingKhach = await _context.Khachhangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == existingUser.MaTaiKhoan);
                    if (existingKhach != null)
                    {
                        _context.Khachhangs.Remove(existingKhach);
                    }

                    // 2. Xóa tài khoản chưa kích hoạt cũ
                    _context.Taikhoans.Remove(existingUser);

                    // 3. Lưu thay đổi tạm thời để giải phóng Username hoàn toàn
                    await _context.SaveChangesAsync();
                }
            }

            // ---- HỆ THỐNG TIẾP TỤC TẠO TÀI KHOẢN MỚI SAU KHI ĐÃ DỌN SẠCH RÁC ----
            string randomOtp = new Random().Next(100000, 999999).ToString();

            var taiKhoanMoi = new Taikhoan
            {
                TenDangNhap = model.Username,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(model.Password),
                VaiTro = "KhachHang",
                IsConfirmed = false, // Mặc định bằng 0 (False), phải qua OTP mới kích hoạt
                OtpCode = randomOtp,
                OtpExpired = DateTime.Now.AddMinutes(5) // Hết hạn sau 5 phút
            };
            _context.Taikhoans.Add(taiKhoanMoi);
            await _context.SaveChangesAsync();

            var khachHangMoi = new Khachhang
            {
                HoTen = model.HoTen,
                DienThoai = model.DienThoai,
                Email = model.Email,
                MaTaiKhoan = taiKhoanMoi.MaTaiKhoan
            };
            _context.Khachhangs.Add(khachHangMoi);
            await _context.SaveChangesAsync();

            // Gửi email chứa mã OTP mới về hòm thư của khách
            SendEmailOTP(model.Email, randomOtp);

            return Ok(new { message = "Đăng ký thành công! Vui lòng kiểm tra Gmail để lấy mã OTP xác thực." });
        }

        // ==========================================
        // 3. API XÁC THỰC MÃ OTP (KÍCH HOẠT TÀI KHOẢN)
        // ==========================================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            var user = await _context.Taikhoans
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username);

            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản!" });

            if (user.OtpCode != model.OtpCode) return BadRequest(new { message = "Mã OTP không chính xác!" });

            if (user.OtpExpired < DateTime.Now) return BadRequest(new { message = "Mã OTP đã hết hạn!" });

            // Nếu mọi thứ đều đúng -> Kích hoạt tài khoản thành công, xóa sạch dấu vết OTP
            user.IsConfirmed = true;
            user.OtpCode = null;
            user.OtpExpired = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xác thực tài khoản thành công! Bây giờ bạn đã có thể đăng nhập." });
        }

        // ==========================================
        // 4. API ĐĂNG NHẬP (CÓ CHECK XÁC THỰC EMAIL)
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var taiKhoan = await _context.Taikhoans
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username);

            if (taiKhoan == null || !BCrypt.Net.BCrypt.Verify(model.Password, taiKhoan.MatKhau))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            // CHẶN KHÔNG CHO LOGIN NẾU CHƯA XÁC THỰC EMAIL (ISCONFIRMED == FALSE HOẶC 0)
            if (taiKhoan.IsConfirmed == false)
            {
                return BadRequest(new { message = "Tài khoản của bạn chưa được xác thực bằng mã OTP trên Gmail!" });
            }

            // --- Giữ nguyên đoạn tạo Token JWT bên dưới của bạn ---
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                    new Claim(ClaimTypes.Role, taiKhoan.VaiTro ?? "NhanVien")
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString, Username = taiKhoan.TenDangNhap, Role = taiKhoan.VaiTro });
        }
    }

    // Các lớp Model bổ sung nhận dữ liệu dữ nguyên
    public class VerifyOtpModel
    {
        public string Username { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
    public class RegisterModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string DienThoai { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
    public class LoginModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}