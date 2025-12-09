using System.Data;

namespace LoanApplication.Application.Common.Contracts.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}