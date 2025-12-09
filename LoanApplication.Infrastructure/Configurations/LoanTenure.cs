namespace LoanApplication.Infrastructure.Configurations;

public record LoanTenure
{
    public const string Path = "LoanTenure";
    public int InterestRate { get; set; }
};