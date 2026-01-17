using LoanApplication.Domain.Entities;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IAuthService
{
    int MaxFailedAttempts { get; }
    int BaseLockoutMinutes { get; }
    int LockoutMultiplier { get; }
    int MaxLockoutMinutes { get; }

    string GetSignedInUserId();
    string GetSignedInUserEmail();
    string GetSignedInUserName();
    Task SendEmailConfirmationAsync(User user, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> CheckPassword(Authentication.CheckPasswordRequest request);
}