using System.Data;
using Dapper;
using LoanApplication.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;

namespace LoanApplication.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(
    IDbConnection connection,
    IDbTransaction? transaction) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        var sql = """
                  SELECT * FROM RefreshTokens 
                           WHERE Token = @token
                           AND IsRevoked = 0
                           AND IsUsed = 0
                           AND ExpiresAt > SWITCHOFFSET(SYSDATETIMEOFFSET(), '+00:00')
                  """;

        var result = await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { token }, transaction);
        return result;
    }
}