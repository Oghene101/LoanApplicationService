using LoanApplication.Application.Common.Contracts.Services;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IHashingKeyStore
{
    GetHashKeyResponse GetActiveKey();
    GetHashKeyResponse GetByKeyId(string keyId);
}