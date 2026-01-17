namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IAuthHeaderProvider
{
    Task<(string Name, string Value)> GetAsync(CancellationToken cancellationToken = default);
}