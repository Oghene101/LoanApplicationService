using LoanApplication.Application.Common.Contracts.Services;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IEncryptionKeyStore
{
    GetEncryptionKeyResponse GetActiveKey();
    GetEncryptionKeyResponse GetByKeyId(string keyId);
}