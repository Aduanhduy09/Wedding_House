using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wedding_House.Models;

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

        // Khai báo Model nhận dữ liệu đăng nhập từ Frontend gửi lên
        public class LoginModel
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // 1. Kiểm tra tài khoản dưới Database 
            var taiKhoan = await _context.Taikhoans
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

            if (taiKhoan == null)
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            // 2. Nếu đúng tài khoản, tiến hành ký và cấp "Thẻ thông hành" JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                    new Claim(ClaimTypes.Role, taiKhoan.VaiTro ?? "NhanVien") // Đóng dấu Vai trò của User vào token (Admin/NhanVien)
                }),
                Expires = DateTime.UtcNow.AddHours(4), // Token có giá trị trong 4 tiếng
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 3. Trả Token và vai trò về cho Frontend
            return Ok(new
            {
                Token = tokenString,
                Username = taiKhoan.TenDangNhap,
                Role = taiKhoan.VaiTro
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Taikhoan model)
        {
            // 1. Kiểm tra xem tên đăng nhập đã tồn tại chưa
            var checkExit = await _context.Taikhoans.AnyAsync(u => u.TenDangNhap == model.TenDangNhap);
            if (checkExit)
            {
                return BadRequest(new { message = "Tài khoản này đã tồn tại trong hệ thống!" });
            }

            // 2. Gán quyền mặc định cho tài khoản đăng ký online là Khách hàng
            model.VaiTro = "KhachHang";

            // LƯU Ý: Tạm thời để mật khẩu thô để test đồng bộ với hàm Login cũ. 
            // Khi nào cài BCrypt nâng cấp bảo mật sau.
            _context.Taikhoans.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }
    }
}