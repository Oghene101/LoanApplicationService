using System.Data;
using Dapper;
using LoanApplication.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;

namespace LoanApplication.Infrastructure.Persistence.Repositories;

internal sealed class AddressRepository(
    IDbConnection connection,
    IDbTransaction? transaction) : IAddressRepository
{
    public async Task<IEnumerable<Address>> GetAddressesAsync(Guid kycVerificationId)
    {
        var sql = """
                  SELECT * FROM Addresses 
                           WHERE KycVerificationId = @kycVerificationId
                  """;

        var result = await connection.QueryAsync<Address>(sql, new { kycVerificationId }, transaction);
        return result;
    }

    public async Task<IEnumerable<Address>> GetMostRecentAddressAsync(Guid kycVerificationId)
    {
        var sql = """
                  SELECT top (1) * FROM Addresses 
                           WHERE KycVerificationId = @kycVerificationId
                           ORDER BY CreatedAt DESC
                  """;

        var result = await connection.QueryAsync<Address>(sql, new { kycVerificationId }, transaction);
        return result;
    }

    public async Task<bool> AddressExistsAsync(Guid kycVerificationId, string houseNumber,
        string street, string city, string state, string country)
    {
        var sql = """
                  SELECT 1 FROM Addresses 
                           WHERE KycVerificationId = @kycVerificationId
                           AND HouseNumber = @houseNumber
                           AND Street = @street
                           AND City = @city
                           AND State = @state
                           AND Country = @country
                  """;

        var result = await connection.ExecuteScalarAsync<int?>(sql,
            new { kycVerificationId, houseNumber, street, city, state, country }, transaction);

        return result.HasValue;
    }
}