using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Api.Domain.Entities;

public class SavingsGoal
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string GoalName { get; set; } = string.Empty;

    [Column(TypeName = "numeric(18,2)")]
    public decimal TargetAmount { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal CurrentAmount { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal Progress { get; set; }

    public User User { get; set; } = null!;
}


