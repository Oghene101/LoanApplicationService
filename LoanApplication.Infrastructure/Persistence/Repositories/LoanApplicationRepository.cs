using Dapper;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;

namespace LoanApplication.Infrastructure.Persistence.Repositories;

public class LoanApplicationRepository(
    IDbConnectionFactory connectionFactory) : ILoanApplicationRepository
{
    public async Task<Domain.Entities.LoanApplication?> GetLoanApplicationByIdAsync(
        Guid id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT * FROM "LoanApplications"
                           Where "Id" = @id
                  """;

        var result =
            await connection.QueryFirstOrDefaultAsync<Domain.Entities.LoanApplication>(sql, new { id });

        return result;
    }

    public async Task<(IEnumerable<Domain.Entities.LoanApplication>, int)> GetPendingLoanApplicationsAsync(
        int pageNumber, int pageSize,
        DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var parameters = new DynamicParameters();
        var whereClause = """ WHERE 1=1 AND LA."ApplicationStatus" = 0 """;

        using var connection = connectionFactory.CreateConnection();

        if (startDate.HasValue && endDate.HasValue)
        {
            whereClause += """ AND LA."CreatedAt" >= @StartDate AND LA."CreatedAt" < @EndDate """;
            parameters.Add("StartDate", startDate.Value);
            parameters.Add("EndDate", endDate.Value);
        }

        parameters.Add("Skip", (pageNumber - 1) * pageSize);
        parameters.Add("Take", pageSize);

        var sql = $"""
                   -- Query 1: paged data
                   SELECT LA.*, U."Id", U."FirstName", U."LastName", U."Email"
                    FROM "LoanApplications" AS LA
                   INNER JOIN "AspNetUsers" AS U ON LA."UserId" = U."Id"
                   {whereClause}
                   ORDER BY LA."CreatedAt" DESC
                   OFFSET @Skip LIMIT @Take;

                   -- Query 2: total count
                   SELECT COUNT(*) FROM "LoanApplications" As LA
                   {whereClause};
                   """;

        await using var reader = await connection.QueryMultipleAsync(sql, parameters);
        var loanApplications =
            reader.Read<Domain.Entities.LoanApplication, User, Domain.Entities.LoanApplication>(
                (loanApplication, user) =>
                {
                    loanApplication.User = user;
                    return loanApplication;
                },
                "Id");

        var totalCount = reader.ReadSingle<int>();

        return (loanApplications, totalCount);
    }
}