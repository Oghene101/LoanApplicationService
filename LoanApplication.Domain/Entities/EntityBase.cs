using System.ComponentModel.DataAnnotations;
using LoanApplication.Domain.Abstractions;

namespace LoanApplication.Domain.Entities;

public abstract class EntityBase : IAuditable
{
    [Required] public Guid Id { get; set; } = Guid.CreateVersion7();
    [Required] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string CreatedBy { get; set; } = string.Empty;
    [Required] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string UpdatedBy { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }
    [MaxLength(150)] public string? DeletedBy { get; set; }
}