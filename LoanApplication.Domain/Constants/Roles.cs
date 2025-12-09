namespace LoanApplication.Domain.Constants;

public class Roles
{
    public const string Admin = "Admin";
    public const string LoanAdmin = "LoanAdmin";
    public const string User = "User";

    public static readonly List<string> List = [Admin, LoanAdmin, User];
}