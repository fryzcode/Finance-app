using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Api.Domain.Entities;

public class Balance
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalBalance { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal ReserveBalance { get; set; }

    public int ReservePercentage { get; set; }

    public User User { get; set; } = null!;
}


