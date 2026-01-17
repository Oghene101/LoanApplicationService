using System.Security.Cryptography;
using System.Text;
using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Application.Common.Contracts.Services;

namespace LoanApplication.Infrastructure.Services.Security;

internal sealed class EncryptionProvider(
    IEncryptionKeyStore keyStore) : IEncryptionProvider
{
    private const int NonceSize = 12; // Recommended for GCM
    private const int TagSize = 16; // 128-bit authentication tag

    public EncryptResponse Encrypt(string plaintext)
    {
        byte[]? plaintextBytes = null;
        try
        {
            plaintext = Normalize(plaintext);
            var key = keyStore.GetActiveKey();

            if (key.Secret.Length != 32)
                throw new InvalidOperationException(
                    $"AES-256 requires a 32-byte key. Provided: {key.Secret.Length} bytes");

            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var cipherText = new byte[plaintextBytes.Length];
            var tag = new byte[TagSize];

            using var aes = new AesGcm(key.Secret, TagSize);
            aes.Encrypt(nonce, plaintextBytes, cipherText, tag);

            // Final payload format:
            // nonce | tag | ciphertext
            var combined = Combine(nonce, tag, cipherText);

            return new EncryptResponse(
                Convert.ToBase64String(combined),
                key.KeyId
            );
        }
        finally
        {
            if (plaintextBytes is not null)
                CryptographicOperations.ZeroMemory(plaintextBytes);
        }
    }

    public string Decrypt(string ciphertext, string keyId)
    {
        byte[]? plaintext = null;
        try
        {
            ciphertext = Normalize(ciphertext);
            var key = keyStore.GetByKeyId(keyId);

            if (key.Secret.Length != 32)
                throw new InvalidOperationException(
                    $"AES-256 requires a 32-byte key. Provided: {key.Secret.Length} bytes");

            var combined = Convert.FromBase64String(ciphertext);

            if (combined.Length < NonceSize + TagSize)
                throw new CryptographicException("Invalid encrypted payload.");

            var nonce = combined[..NonceSize];
            var tag = combined[NonceSize..(NonceSize + TagSize)];
            var cipher = combined[(NonceSize + TagSize)..];

            plaintext = new byte[cipher.Length];

            using var aes = new AesGcm(key.Secret, TagSize);
            aes.Decrypt(nonce, cipher, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        finally
        {
            if (plaintext is not null)
                CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private static string Normalize(string text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? throw new ArgumentException("Text cannot be null or empty.")
            : text.Trim();
    }

    private static byte[] Combine(byte[] nonce, byte[] tag, byte[] cipher)
    {
        var result = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, result, nonce.Length + tag.Length, cipher.Length);
        return result;
    }
}