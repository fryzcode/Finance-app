using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Api.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Profession { get; set; }

    public decimal Salary { get; set; }

    public decimal AdditionalEarnings { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    public int? SalaryDay { get; set; }

    public DateTime RegistrationDate { get; set; }

    [MaxLength(1024)]
    public string? AvatarUrl { get; set; }

    [MaxLength(256)]
    public string? PinHash { get; set; }

    public Balance? Balance { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<NonExcludableNeed> NonExcludableNeeds { get; set; } = new List<NonExcludableNeed>();

    public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
}


