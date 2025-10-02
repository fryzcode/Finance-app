using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Api.Domain.Entities;

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

public class Transaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public User User { get; set; } = null!;
}


