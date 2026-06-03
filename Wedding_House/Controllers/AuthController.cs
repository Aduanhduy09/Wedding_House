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
        // 1. HÀM TRỢ GIÚP: GỬI EMAIL MÃ OTP ĐĂNG KÝ
        // ==========================================
        private void SendEmailOTP(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
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
        // 1.5 HÀM TRỢ GIÚP: GỬI EMAIL KHÔI PHỤC MẬT KHẨU
        // ==========================================
        private void SendEmailResetPassword(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Nhà hàng Wedding House", "weddinghouseadmin@gmail.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Mã Khôi Phục Mật Khẩu - Wedding House";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px;'>
                        <h2 style='color: #0e1b2e;'>Khôi phục mật khẩu</h2>
                        <p>Bạn vừa yêu cầu khôi phục mật khẩu trên hệ thống Wedding House.</p>
                        <p>Mã xác nhận (OTP) của bạn là:</p>
                        <div style='background-color: #f1f5f9; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #2563eb; border-radius: 8px; margin: 20px 0;'>
                            {otpCode}
                        </div>
                        <p style='color: #64748b; font-size: 13px;'>Tuyệt đối không chia sẻ mã này cho bất kỳ ai. Mã này sẽ giúp bạn thiết lập lại mật khẩu mới.</p>
                    </div>"
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
            var isEmailExist = await _context.Khachhangs.AnyAsync(k => k.Email == model.Email);
            if (isEmailExist)
            {
                return BadRequest(new { message = "Email này đã được sử dụng bởi một tài khoản khác!" });
            }

            var isPhoneExist = await _context.Khachhangs.AnyAsync(k => k.DienThoai == model.DienThoai);
            if (isPhoneExist)
            {
                return BadRequest(new { message = "Số điện thoại này đã được đăng ký hệ thống!" });
            }

            var existingUser = await _context.Taikhoans.FirstOrDefaultAsync(u => u.TenDangNhap == model.Username);
            if (existingUser != null)
            {
                if (existingUser.IsConfirmed == true || existingUser.IsConfirmed == null)
                {
                    return BadRequest(new { message = "Tài khoản này đã tồn tại và đã được xác thực trong hệ thống!" });
                }
                else
                {
                    var existingKhach = await _context.Khachhangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == existingUser.MaTaiKhoan);
                    if (existingKhach != null)
                    {
                        _context.Khachhangs.Remove(existingKhach);
                    }
                    _context.Taikhoans.Remove(existingUser);
                    await _context.SaveChangesAsync();
                }
            }

            string randomOtp = new Random().Next(100000, 999999).ToString();
            var taiKhoanMoi = new Taikhoan
            {
                TenDangNhap = model.Username,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(model.Password),
                VaiTro = "KhachHang",
                IsConfirmed = false,
                OtpCode = randomOtp,
                OtpExpired = DateTime.Now.AddMinutes(5)
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

            SendEmailOTP(model.Email, randomOtp);
            return Ok(new { message = "Đăng ký thành công! Vui lòng kiểm tra Gmail để lấy mã OTP xác thực." });
        }

        // ==========================================
        // 3. API XÁC THỰC MÃ OTP (KÍCH HOẠT TÀI KHOẢN)
        // ==========================================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            var user = await _context.Taikhoans.FirstOrDefaultAsync(u => u.TenDangNhap == model.Username);

            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản!" });
            if (user.OtpCode != model.OtpCode) return BadRequest(new { message = "Mã OTP không chính xác!" });
            if (user.OtpExpired < DateTime.Now) return BadRequest(new { message = "Mã OTP đã hết hạn!" });

            user.IsConfirmed = true;
            user.OtpCode = null;
            user.OtpExpired = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xác thực tài khoản thành công! Bây giờ bạn đã có thể đăng nhập." });
        }

        // ==========================================
        // 4. API ĐĂNG NHẬP
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var taiKhoan = await _context.Taikhoans.FirstOrDefaultAsync(u => u.TenDangNhap == model.Username);

            if (taiKhoan == null || !BCrypt.Net.BCrypt.Verify(model.Password, taiKhoan.MatKhau))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            if (taiKhoan.IsConfirmed == false)
            {
                return BadRequest(new { message = "Tài khoản của bạn chưa được xác thực bằng mã OTP trên Gmail!" });
            }

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

        // ==========================================
        // 5. API KIỂM TRA EMAIL VÀ GỬI MÃ OTP QUÊN MẬT KHẨU
        // ==========================================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var khachHang = await _context.Khachhangs.FirstOrDefaultAsync(k => k.Email == request.Email);
                if (khachHang == null)
                {
                    return BadRequest(new { message = "Gmail đăng ký không hợp lệ hoặc chưa tồn tại!" });
                }

                var taiKhoan = await _context.Taikhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == khachHang.MaTaiKhoan);
                if (taiKhoan == null)
                {
                    return BadRequest(new { message = "Lỗi hệ thống: Không tìm thấy tài khoản liên kết với khách hàng này!" });
                }

                string otpCode = new Random().Next(100000, 999999).ToString();

                taiKhoan.OtpCode = otpCode;
                await _context.SaveChangesAsync();

                // Gọi hàm gửi email khôi phục mật khẩu
                SendEmailResetPassword(khachHang.Email, otpCode);

                return Ok(new { message = "Mã xác nhận đã được gửi đến email của bạn!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==========================================
        // 6. API XÁC NHẬN OTP VÀ ĐỔI MẬT KHẨU
        // ==========================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var khachHang = await _context.Khachhangs.FirstOrDefaultAsync(k => k.Email == request.Email);
                if (khachHang == null) return BadRequest(new { message = "Email không hợp lệ!" });

                var taiKhoan = await _context.Taikhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == khachHang.MaTaiKhoan);
                if (taiKhoan == null) return BadRequest(new { message = "Không tìm thấy tài khoản!" });

                if (taiKhoan.OtpCode == null || taiKhoan.OtpCode != request.Otp)
                {
                    return BadRequest(new { message = "Mã xác nhận không chính xác!" });
                }

                taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                taiKhoan.OtpCode = null; // Xóa OTP đi để không bị dùng lại

                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // --- CÁC CLASS DTO HỨNG DỮ LIỆU ---
        public class ForgotPasswordRequest { public string Email { get; set; } }
        public class ResetPasswordRequest
        {
            public string Email { get; set; }
            public string Otp { get; set; }
            public string NewPassword { get; set; }
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