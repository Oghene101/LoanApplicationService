namespace LoanApplication.Application.Common.Contracts.Abstractions.Security;

public interface IMaskingService
{
    string MaskBvnOrNin(string bvn);
}