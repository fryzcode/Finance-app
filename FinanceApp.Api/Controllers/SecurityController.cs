using System.Security.Claims;
using FinanceApp.Api.Application.Auth;
using FinanceApp.Api.Application.Email;
using FinanceApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _email;

    public SecurityController(ApplicationDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    [HttpPost("pin")] // set or update PIN
    public async Task<IActionResult> SetPin([FromBody] string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length is < 4 or > 8 || !pin.All(char.IsDigit))
            return BadRequest(new { message = "PIN must be 4-8 digits" });

        var user = await GetUserAsync();
        if (user == null) return NotFound();

        user.PinHash = PasswordHasher.HashPassword(pin);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("pin/verify")]
    public async Task<IActionResult> VerifyPin([FromBody] string pin)
    {
        var user = await GetUserAsync();
        if (user == null || string.IsNullOrEmpty(user.PinHash)) return Unauthorized();

        var ok = PasswordHasher.VerifyPassword(pin, user.PinHash);
        return ok ? Ok(new { valid = true }) : Unauthorized();
    }

    [AllowAnonymous]
    [HttpPost("password/reset-request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized);
        if (user != null)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-");
            var expires = DateTime.UtcNow.AddHours(1);
            _db.PasswordResetTokens.Add(new Domain.Entities.PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                ExpiresAtUtc = expires,
                Used = false
            });
            await _db.SaveChangesAsync();

            var resetUrl = $"https://yourapp/reset-password?token={token}&email={Uri.EscapeDataString(normalized)}";
            await _email.SendAsync(normalized, "Password Reset", $"Click to reset your password: {resetUrl}", false);
        }
        return Ok(new { message = "If the email exists, a reset link was sent." });
    }

    [AllowAnonymous]
    [HttpPost("password/reset-confirm")]
    public async Task<IActionResult> ConfirmPasswordReset([FromQuery] string email, [FromQuery] string token, [FromBody] string newPassword)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized);
        if (user == null) return BadRequest(new { message = "Invalid request" });

        var record = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.Token == token && !t.Used && t.ExpiresAtUtc > DateTime.UtcNow)
            .FirstOrDefaultAsync();
        if (record == null) return BadRequest(new { message = "Invalid or expired token" });

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        record.Used = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Domain.Entities.User?> GetUserAsync()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.Parse(sub!);
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}

