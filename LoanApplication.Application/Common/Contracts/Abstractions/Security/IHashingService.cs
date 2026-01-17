using LoanApplication.Application.Common.Contracts.Services;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IHashingService
{
    ComputeHashResponse Compute(string input);
    ComputeHashResponse Compute(string input, string keyId);
    bool Verify(string input, string storedHash, string keyId);
}