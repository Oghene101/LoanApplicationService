using LoanApplication.Application.Common.Contracts.Abstractions.Security;

namespace LoanApplication.Infrastructure.Services.Security;

internal sealed class MaskingService : IMaskingService
{
    public string MaskBvnOrNin(string bvn)
    {
        if (string.IsNullOrWhiteSpace(bvn))
            return "***";

        const int prefixLength = 4;
        const int suffixLength = 3;

        if (bvn.Length <= prefixLength + suffixLength)
            return new string('*', bvn.Length);

        var prefix = bvn[..prefixLength];
        var suffix = bvn[^suffixLength..];
        var maskedLength = bvn.Length - (prefixLength + suffixLength);

        return $"{prefix}{new string('*', maskedLength)}{suffix}";
    }
}