using System.ComponentModel.DataAnnotations;
using LoanApplication.Domain.Enums;

namespace LoanApplication.Domain.Entities;

public class LoanApplicationHistory : EntityBase
{
    [Required]
    public LoanApplicationStatus ApplicationStatus
    {
        get;
        set
        {
            field = value;
            ValidateComment();
        }
    }
    
    [MaxLength(1000)]
    public string? Comment
    {
        get;
        set
        {
            field = value;
            ValidateComment();
        }
    }

    public Guid LoanApplicationId { get; set; }

    //Navigation property
    public LoanApplication LoanApplication { get; set; } = null!;

    private void ValidateComment()
    {
        if (ApplicationStatus is LoanApplicationStatus.Rejected &&
            string.IsNullOrWhiteSpace(Comment))
        {
            throw new ValidationException("Comment is required when application is rejected");
        }
    }
}