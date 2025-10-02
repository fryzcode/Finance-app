using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Api.Domain.Entities;

public class BalanceHistory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Column(TypeName = "numeric(18,2)")]
    public decimal Balance { get; set; }

    [Required]
    [Column(TypeName = "numeric(18,2)")]
    public decimal ReserveBalance { get; set; }

    [Required]
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalIncome { get; set; }

    [Required]
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalExpenses { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
