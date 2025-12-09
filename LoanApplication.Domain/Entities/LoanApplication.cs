using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoanApplication.Domain.Enums;

namespace LoanApplication.Domain.Entities;

public class LoanApplication : EntityBase
{
    [Required, Column(TypeName = "decimal(18,0)")]
    public decimal Amount { get; set; }

    [Required] public int Tenure { get; set; }
    [Required, MaxLength(500)] public string Purpose { get; set; } = string.Empty;
    [Required] public LoanApplicationStatus ApplicationStatus { get; set; } = LoanApplicationStatus.Pending;
    [Required] public Guid UserId { get; set; }

    //Navigation Property
    public User User { get; set; } = null!;
    public ICollection<LoanApplicationHistory> LoanApplicationHistories { get; set; } = [];
}