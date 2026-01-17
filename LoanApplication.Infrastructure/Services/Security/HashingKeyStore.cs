using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Application.Common.Contracts.Services;
using LoanApplication.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace LoanApplication.Infrastructure.Services.Security;

internal sealed class HashingKeyStore : IHashingKeyStore
{
    private readonly IReadOnlyList<GetHashKeyResponse> _keys;

    public HashingKeyStore(
        IOptions<HashingSettings> options)
    {
        _keys = options.Value.Keys
            .Select(k => new GetHashKeyResponse(
                k.KeyId,
                Convert.FromBase64String(k.Secret),
                k.IsActive))
            .ToList();

        Validate();
    }

    public GetHashKeyResponse GetActiveKey()
        => _keys.Single(k => k.IsActive);

    public GetHashKeyResponse GetByKeyId(string keyId)
        => _keys.Single(k => k.KeyId == keyId);

    private void Validate()
    {
        if (_keys.Count == 0)
            throw new InvalidOperationException("No HMAC keys configured.");

        if (_keys.Count(k => k.IsActive) != 1)
            throw new InvalidOperationException("Exactly one HMAC key must be active.");

        var duplicateIds = _keys
            .GroupBy(k => k.KeyId)
            .Any(g => g.Count() > 1);

        if (duplicateIds)
            throw new InvalidOperationException("Duplicate HMAC KeyId detected.");
    }
}