using FamilyPillsAPI.Data;
using FamilyPillsAPI.Models;
using FamilyPillsAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
// using FamilyPillsAPI.Data; // Thay bằng namespace chứa DbContext của bạn

namespace FamilyPillsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Cờ này bắt buộc request phải có Header "Authorization: Bearer <token>"
    public class UserController : ControllerBase // Hoặc kế thừa BaseController nếu bạn có sẵn custom response
    {
        private readonly FamilyPillsDbContext _context; // Đổi thành tên DbContext thực tế của bạn

        public UserController(FamilyPillsDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetMyProfile()
        {
            // 1. Trích xuất UserId từ Token Claims
            // Tùy thuộc vào lúc GenerateToken bạn dùng ClaimTypes nào, thường là NameIdentifier hoặc "id"
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                // Sử dụng custom response trả về lỗi 401 nếu token không hợp lệ
                return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin User." });
            }

            // 2. Truy vấn Database dựa trên userId lấy từ token
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            // 3. Tính toán các dữ liệu phụ thuộc (Ví dụ: số lượng thuốc của user này)
            // Giả sử bạn có bảng Medicines và có liên kết với User
            // int medicineCount = await _context.Medicines.CountAsync(m => m.UserId == userId);
            int medicineCount = 0; // Tạm gán bằng 0, bạn thay bằng logic query thực tế

            // 4. Map dữ liệu sang DTO Profile 
            var profileData = new UserProfileResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLogin = user.UpdatedAt, // Tạm dùng UpdatedAt nếu bảng User không có cột LastLogin
                MedicineCount = medicineCount
            };

            // 5. Trả về Response
            // Nếu bạn có base method SuccessResponse (như trong hàm Login của bạn), hãy bọc nó lại.
            // Ví dụ: return SuccessResponse(profileData, "Lấy thông tin thành công");

            return Ok(new { data = profileData, message = "Lấy thông tin thành công" });
        }

        /// <summary>
        /// API 1: Đổi mật khẩu người dùng
        /// </summary>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // 1. Trích xuất UserId từ Token Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn." });
            }

            // 2. Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Mật khẩu cũ và mật khẩu mới không được để trống." });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
            }

            // 3. Tìm user trong cơ sở dữ liệu
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // 4. Kiểm tra mật khẩu hiện tại bằng BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
            {
                return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });
            }

            // 5. Mã hóa mật khẩu mới và cập nhật thời gian
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        /// <summary>
        /// API 2: Đổi họ và tên người dùng
        /// </summary>
        [HttpPut("change-name")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeNameRequest request)
        {
            // 1. Trích xuất UserId từ Token Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn." });
            }

            // 2. Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(request.NewFullName))
            {
                return BadRequest(new { message = "Họ và tên mới không được để trống." });
            }

            // 3. Tìm user trong cơ sở dữ liệu
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // 4. Cập nhật thông tin tên mới
            user.FullName = request.NewFullName.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật họ tên thành công.", data = new { fullName = user.FullName } });
        }
    }
}