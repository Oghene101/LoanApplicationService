namespace LoanApplication.Infrastructure.Configurations;

public record LoanTenureSettings
{
    public const string Path = "LoanTenure";
    public int InterestRate { get; set; }
};