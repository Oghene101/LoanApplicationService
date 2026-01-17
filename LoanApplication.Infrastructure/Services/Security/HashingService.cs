using System.Security.Cryptography;
using System.Text;
using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Application.Common.Contracts.Services;

namespace LoanApplication.Infrastructure.Services.Security;

internal sealed class HashingService(
    IHashingKeyStore keyStore) : IHashingService
{
    public ComputeHashResponse Compute(string input)
    {
        input = Normalize(input);

        var key = keyStore.GetActiveKey();
        var hash = ComputeInternal(input, key.Secret);

        return new ComputeHashResponse(hash, key.KeyId);
    }

    public ComputeHashResponse Compute(string input, string keyId)
    {
        input = Normalize(input);

        var key = keyStore.GetByKeyId(keyId);
        var hash = ComputeInternal(input, key.Secret);

        return new ComputeHashResponse(hash, key.KeyId);
    }

    public bool Verify(string input, string storedHash, string keyId)
    {
        input = Normalize(input);

        var key = keyStore.GetByKeyId(keyId);
        var computed = ComputeInternal(input, key.Secret);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(storedHash),
            Convert.FromBase64String(computed)
        );
    }

    private static string ComputeInternal(string input, byte[] secret)
    {
        using var hmac = new HMACSHA256(secret);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private static string Normalize(string input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? throw new ArgumentException("Input cannot be null or empty.")
            : input.Trim();
    }
}