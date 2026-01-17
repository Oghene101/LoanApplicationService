using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Application.Common.Contracts.Services;
using LoanApplication.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace LoanApplication.Infrastructure.Services.Security;

internal sealed class EncryptionKeyStore : IEncryptionKeyStore
{
    private readonly IReadOnlyList<GetEncryptionKeyResponse> _keys;

    public EncryptionKeyStore(
        IOptions<EncryptionSettings> options)
    {
        _keys = options.Value.Keys
            .Select(k => new GetEncryptionKeyResponse(
                k.KeyId,
                Convert.FromBase64String(k.Secret),
                k.IsActive))
            .ToList();

        Validate();
    }

    public GetEncryptionKeyResponse GetActiveKey()
        => _keys.Single(k => k.IsActive);

    public GetEncryptionKeyResponse GetByKeyId(string keyId)
        => _keys.Single(k => k.KeyId == keyId);

    private void Validate()
    {
        if (_keys.Count == 0)
            throw new InvalidOperationException("No AES keys configured.");

        if (_keys.Count(k => k.IsActive) != 1)
            throw new InvalidOperationException("Exactly one AES key must be active.");

        var duplicateIds = _keys
            .GroupBy(k => k.KeyId)
            .Any(g => g.Count() > 1);

        if (duplicateIds)
            throw new InvalidOperationException("Duplicate AES KeyId detected.");
    }
}