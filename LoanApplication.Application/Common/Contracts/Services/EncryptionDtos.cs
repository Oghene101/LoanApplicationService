namespace LoanApplication.Application.Common.Contracts.Services;

public record GetEncryptionKeyResponse(
    string KeyId,
    byte[] Secret,
    bool IsActive);

public record EncryptResponse(
    string CipherText,
    string KeyId);