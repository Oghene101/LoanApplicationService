using System.ComponentModel.DataAnnotations;
using LoanApplication.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace LoanApplication.Domain.Entities;

public class User : IdentityUser<Guid>, IAuditable
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
    public int LockoutCount { get; set; }
    [Required] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string CreatedBy { get; set; } = string.Empty;
    [Required] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string UpdatedBy { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    //Navigation property
    public ICollection<LoanApplication> LoanApplications { get; set; } = [];
}