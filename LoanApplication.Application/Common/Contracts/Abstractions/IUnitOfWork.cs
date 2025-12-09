using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;

namespace LoanApplication.Application.Common.Contracts.Abstractions;

public interface IUnitOfWork
{
    public IRefreshTokenRepository RefreshTokensReadRepository { get; }
    public IRepository<RefreshToken> RefreshTokensWriteRepository { get; }
    public IKycVerificationRepository KycVerificationsReadRepository { get; }
    public IRepository<KycVerification> KycVerificationsWriteRepository { get; }
    public IAddressRepository AddressesReadRepository { get; }
    public IRepository<Address> AddressesWriteRepository { get; }
    public ILoanApplicationRepository LoanApplicationsReadRepository { get; }
    public IRepository<Domain.Entities.LoanApplication> LoanApplicationsWriteRepository { get; }
    public IRepository<LoanApplicationHistory> LoanApplicationHistoryWriteRepository { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}