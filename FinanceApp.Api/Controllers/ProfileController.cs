using System.Security.Claims;
using FinanceApp.Api.Application.Auth;
using FinanceApp.Api.Contracts.Profile;
using FinanceApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProfileController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileResponse>> Get()
        {
            var userId = GetUserId();
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            return new ProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Profession = user.Profession,
                Salary = user.Salary,
                AdditionalEarnings = user.AdditionalEarnings,
                Currency = user.Currency,
                SalaryDay = user.SalaryDay,
                RegistrationDate = user.RegistrationDate,
                AvatarUrl = user.AvatarUrl
            };
        }

        [HttpPut]
        public async Task<ActionResult<ProfileResponse>> Update([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Surname))
                user.Surname = request.Surname;

            if (!string.IsNullOrWhiteSpace(request.Profession))
                user.Profession = request.Profession;

            if (request.Salary.HasValue)
                user.Salary = request.Salary.Value;

            if (request.AdditionalEarnings.HasValue)
                user.AdditionalEarnings = request.AdditionalEarnings.Value;

            if (!string.IsNullOrWhiteSpace(request.Currency))
                user.Currency = request.Currency;

            if (request.SalaryDay.HasValue)
                user.SalaryDay = request.SalaryDay.Value;

            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            await _db.SaveChangesAsync();

            return new ProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Profession = user.Profession,
                Salary = user.Salary,
                AdditionalEarnings = user.AdditionalEarnings,
                Currency = user.Currency,
                SalaryDay = user.SalaryDay,
                RegistrationDate = user.RegistrationDate,
                AvatarUrl = user.AvatarUrl
            };
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserId();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Verify current password
            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Current password is incorrect");
            }

            // Update password
            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private Guid GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirst("sub")?.Value;
            return Guid.Parse(sub!);
        }
    }
}
