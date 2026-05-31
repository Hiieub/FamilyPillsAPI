using System.Security.Claims;
using FamilyPillsAPI.Data;
using FamilyPillsAPI.Models;
using FamilyPillsAPI.Models.Dto;
using FamilyPillsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyPillsAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly FamilyPillsDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(FamilyPillsDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequestResponse<AuthResponse>("Email khong duoc de trong", "EMAIL_REQUIRED", "email");
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email))
            {
                return BadRequestResponse<AuthResponse>("Email khong hop le", "EMAIL_INVALID", "email");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                return BadRequestResponse<AuthResponse>("Mat khau phai co it nhat 6 ky tu", "PASSWORD_TOO_SHORT", "password");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (exists)
            {
                return BadRequestResponse<AuthResponse>("Email da duoc su dung", "EMAIL_EXISTS", "email");
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Email = normalizedEmail,
                FullName = request.FullName?.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = CreateAuthResponse(user);
            return CreatedResponse(response, "Dang ky thanh cong");
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return UnauthorizedResponse<AuthResponse>("Email hoac mat khau khong dung", "INVALID_CREDENTIALS");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return UnauthorizedResponse<AuthResponse>("Email hoac mat khau khong dung", "INVALID_CREDENTIALS");
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return SuccessResponse(CreateAuthResponse(user), "Dang nhap thanh cong");
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<object>> Logout()
        {
            return SuccessResponse<object>(new { }, "Dang xuat thanh cong");
        }

        [Authorize]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Refresh()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return UnauthorizedResponse<TokenResponse>("Token khong hop le", "INVALID_TOKEN");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return UnauthorizedResponse<TokenResponse>("Nguoi dung khong ton tai", "USER_NOT_FOUND");
            }

            var token = _jwtTokenService.GenerateToken(user);
            return SuccessResponse(new TokenResponse
            {
                Token = token.Token,
                TokenExpiry = token.ExpiresAt
            }, "Lam moi token thanh cong");
        }

        private AuthResponse CreateAuthResponse(User user)
        {
            var token = _jwtTokenService.GenerateToken(user);
            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Token = token.Token,
                TokenExpiry = token.ExpiresAt
            };
        }
    }
}
