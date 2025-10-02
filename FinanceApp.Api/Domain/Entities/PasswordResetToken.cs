using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Api.Domain.Entities;

public class PasswordResetToken
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool Used { get; set; }

    public User User { get; set; } = null!;
}


