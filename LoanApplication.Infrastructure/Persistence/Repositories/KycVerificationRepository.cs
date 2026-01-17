using System.Data;
using Dapper;
using LoanApplication.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;

namespace LoanApplication.Infrastructure.Persistence.Repositories;

internal sealed class KycVerificationRepository(
    IDbConnection connection,
    IDbTransaction? transaction) : IKycVerificationRepository
{
    public async Task<KycVerification?> GetKycVerificationAsync(Guid userId)
    {
        var sql = """
                  SELECT * FROM KycVerifications 
                           WHERE UserId = @userId
                  """;

        var result = await connection.QuerySingleOrDefaultAsync<KycVerification>(sql, new { userId }, transaction);
        return result;
    }

    public async Task<KycVerification?> GetKycVerificationWithAddressesAsync(Guid userId)
    {
        var sql = """
                  SELECT * FROM KycVerifications as K
                           LEFT JOIN Addresses as A ON K.Id = A.KycVerificationId
                           WHERE UserId = @userId
                  """;

        var dictionary = new Dictionary<Guid, KycVerification>();
        await connection.QueryAsync<KycVerification, Address, KycVerification>(
            sql,
            (kyc, address) =>
            {
                if (!dictionary.TryGetValue(kyc.Id, out var currentKyc))
                {
                    currentKyc = kyc;
                    dictionary.Add(kyc.Id, currentKyc);
                }

                if (address is not null)
                {
                    currentKyc.Addresses.Add(address);
                }

                return currentKyc;
            },
            new { userId },
            transaction,
            splitOn: "Id");

        return dictionary.Values.SingleOrDefault();
    }
}