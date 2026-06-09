using FamilyPillsAPI.Data;
using FamilyPillsAPI.Models;
using FamilyPillsAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FamilyPillsAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly FamilyPillsDbContext _context;

        public UserController(FamilyPillsDbContext context)
        {
            _context = context;
        }

        // ===== Helper to extract userId from JWT =====
        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            return claim != null && int.TryParse(claim.Value, out userId);
        }

        /// <summary>
        /// GET /api/users/profile – Return authenticated user's profile + medicine count.
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetMyProfile()
        {
            if (!TryGetUserId(out int userId))
                return UnauthorizedResponse<UserProfileResponse>("Token không hợp lệ hoặc thiếu thông tin User.", "INVALID_TOKEN");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFoundResponse<UserProfileResponse>("Không tìm thấy tài khoản.", "USER_NOT_FOUND");

            var profileData = new UserProfileResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLogin = user.UpdatedAt
            };

            return SuccessResponse(profileData, "Lấy thông tin thành công");
        }

        /// <summary>
        /// PUT /api/users/profile – Update the authenticated user's full name.
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> UpdateProfile([FromBody] ChangeNameRequest request)
        {
            if (!TryGetUserId(out int userId))
                return UnauthorizedResponse<UserProfileResponse>("Token không hợp lệ hoặc hết hạn.", "INVALID_TOKEN");

            if (string.IsNullOrWhiteSpace(request.NewFullName))
                return BadRequestResponse<UserProfileResponse>("Họ và tên không được để trống.", "NAME_REQUIRED", "newFullName");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFoundResponse<UserProfileResponse>("Không tìm thấy người dùng.", "USER_NOT_FOUND");

            user.FullName = request.NewFullName.Trim();
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var profileData = new UserProfileResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLogin = user.UpdatedAt
            };

            return SuccessResponse(profileData, "Cập nhật họ tên thành công.");
        }

        /// <summary>
        /// POST /api/users/change-password – Verify current password, then save new hashed password.
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!TryGetUserId(out int userId))
                return UnauthorizedResponse<object>("Token không hợp lệ hoặc hết hạn.", "INVALID_TOKEN");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequestResponse<object>("Mật khẩu cũ và mật khẩu mới không được để trống.", "FIELDS_REQUIRED");

            if (request.NewPassword.Length < 6)
                return BadRequestResponse<object>("Mật khẩu mới phải có ít nhất 6 ký tự.", "PASSWORD_TOO_SHORT", "newPassword");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFoundResponse<object>("Không tìm thấy người dùng.", "USER_NOT_FOUND");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                return BadRequestResponse<object>("Mật khẩu hiện tại không chính xác.", "WRONG_PASSWORD", "currentPassword");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return SuccessResponse<object>(new { }, "Đổi mật khẩu thành công.");
        }
    }
}