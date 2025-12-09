using System.Data;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Contracts.Abstractions;
using Npgsql;

namespace LoanApplication.Infrastructure.Persistence;

public class SqlConnectionFactory(
    string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}