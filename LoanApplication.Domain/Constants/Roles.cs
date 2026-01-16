namespace LoanApplication.Domain.Constants;

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string LoanAdmin = nameof(LoanAdmin);
    public const string User = nameof(User);

    public static readonly string[] List = [Admin, LoanAdmin, User];
}