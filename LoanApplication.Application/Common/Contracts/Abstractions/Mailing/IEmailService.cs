namespace LoanApplication.Application.Common.Contracts.Abstractions.Mailing;

public interface IEmailService
{
    Task SendAsync(string recipientName, string recipientEmail, string subject, string body,
        CancellationToken cancellationToken = default);
}