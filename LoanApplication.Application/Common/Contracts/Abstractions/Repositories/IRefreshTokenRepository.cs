using LoanApplication.Domain.Entities;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
}