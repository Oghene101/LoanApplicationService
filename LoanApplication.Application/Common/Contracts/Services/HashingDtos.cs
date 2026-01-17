namespace LoanApplication.Application.Common.Contracts.Services;

public record GetHashKeyResponse(
    string KeyId,
    byte[] Secret,
    bool IsActive);

public record ComputeHashResponse(
    string Hash,
    string KeyId);