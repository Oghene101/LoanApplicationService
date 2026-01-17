using LoanApplication.Application.Common.Contracts.Services;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IEncryptionProvider
{
    EncryptResponse Encrypt(string plaintext);
    string Decrypt(string ciphertext, string keyId);
}