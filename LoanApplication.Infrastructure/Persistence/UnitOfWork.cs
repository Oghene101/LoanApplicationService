using System.Data;
using System.Diagnostics.CodeAnalysis;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Persistence.DbContexts;
using LoanApplication.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoanApplication.Infrastructure.Persistence;

public class UnitOfWork(
    AppDbContext context) : IUnitOfWork, IAsyncDisposable
{
    private IDbContextTransaction? _transaction;
    public IDbConnection DbConnection => context.Database.GetDbConnection();
    public IDbTransaction? DbTransaction => _transaction?.GetDbTransaction();


    # region Repositories

    #region RefreshTokens

    [field: AllowNull, MaybeNull]
    public IRefreshTokenRepository RefreshTokensReadRepository =>
        field ??= new RefreshTokenRepository(DbConnection, DbTransaction);

    [field: AllowNull, MaybeNull]
    public IRepository<RefreshToken> RefreshTokensWriteRepository =>
        field ??= new Repository<RefreshToken>(context);

    #endregion

    #region KycVerifications

    [field: AllowNull, MaybeNull]
    public IKycVerificationRepository KycVerificationsReadRepository =>
        field ??= new KycVerificationRepository(DbConnection, DbTransaction);

    [field: AllowNull, MaybeNull]
    public IRepository<KycVerification> KycVerificationsWriteRepository =>
        field ??= new Repository<KycVerification>(context);

    #endregion

    #region Addresses

    [field: AllowNull, MaybeNull]
    public IAddressRepository AddressesReadRepository =>
        field ??= new AddressRepository(DbConnection, DbTransaction);

    [field: AllowNull, MaybeNull]
    public IRepository<Address> AddressesWriteRepository =>
        field ??= new Repository<Address>(context);

    #endregion

    #region LoanApplications

    [field: AllowNull, MaybeNull]
    public ILoanApplicationRepository LoanApplicationsReadRepository =>
        field ??= new LoanApplicationRepository(DbConnection, DbTransaction);

    [field: AllowNull, MaybeNull]
    public IRepository<Domain.Entities.LoanApplication> LoanApplicationsWriteRepository =>
        field ??= new Repository<Domain.Entities.LoanApplication>(context);

    #endregion

    #region LoanApplicationHistory

    [field: AllowNull, MaybeNull]
    public IRepository<LoanApplicationHistory> LoanApplicationHistoryWriteRepository =>
        field ??= new Repository<LoanApplicationHistory>(context);

    #endregion

    # endregion

    # region Transaction support (EF + Dapper can share the same transaction if needed)

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null) throw new InvalidOperationException("There is already an active transaction.");

        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) throw new InvalidOperationException("There is no active transaction.");
        await SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) throw new InvalidOperationException("There is no active transaction.");
        await _transaction.RollbackAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
    }

    #endregion
}