using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Api.Domain.Entities;

public class NonExcludableNeed
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Column(TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }

    public User User { get; set; } = null!;
}


