using System.ComponentModel.DataAnnotations;

namespace LoanApplication.Domain.Entities;

public class Address : EntityBase
{
    [Required, MaxLength(10)] public string HouseNumber { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Landmark { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Street { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Lga { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string City { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string State { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Country { get; set; } = string.Empty;
    [Required] public bool IsSuccessfullyVerified { get; set; }
    [Required] public Guid KycVerificationId { get; set; }

    // Navigation props
    public KycVerification KycVerification { get; set; } = null!;
}