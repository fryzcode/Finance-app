using FinanceApp.Api.Application.Auth;
using FinanceApp.Api.Contracts.Auth;
using FinanceApp.Api.Domain.Entities;
using FinanceApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            request.Email = request.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict(new { message = "Email already registered" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Surname = request.Surname.Trim(),
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Profession = request.Profession,
                Salary = request.Salary,
                AdditionalEarnings = request.AdditionalEarnings,
                Currency = request.Currency,
                SalaryDay = request.SalaryDay,
                RegistrationDate = DateTime.UtcNow,
                Balance = new Balance
                {
                    UserId = Guid.Empty, // will be set by EF
                    TotalBalance = 0,
                    ReserveBalance = 0,
                    ReservePercentage = 10
                }
            };

            user.Balance.UserId = user.Id;

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var (token, expires) = _tokenService.CreateToken(user.Id, user.Email);
            return Ok(new AuthResponse { Token = token, ExpiresAtUtc = expires });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var (token, expires) = _tokenService.CreateToken(user.Id, user.Email);
            return Ok(new AuthResponse { Token = token, ExpiresAtUtc = expires });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // With stateless JWT, logout is client-side (remove token). Placeholder for blacklist if needed.
            return Ok(new { message = "logged-out" });
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            return StatusCode(501, new { message = "Google OAuth not implemented" });
        }

        [HttpPost("apple-login")]
        [AllowAnonymous]
        public IActionResult AppleLogin()
        {
            return StatusCode(501, new { message = "Apple OAuth not implemented" });
        }

        [HttpGet("reset-password")]
        [AllowAnonymous]
        public IActionResult ResetPassword([FromQuery] string email)
        {
            // TODO: integrate email service
            return Ok(new { message = "password-reset-link-sent-if-exists" });
        }
    }
}
