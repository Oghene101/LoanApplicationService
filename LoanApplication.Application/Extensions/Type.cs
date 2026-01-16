namespace LoanApplication.Application.Extensions;

public static class TypeExtensions
{
    public static string GetOuterAndInnerName(this Type type)
        => type.FullName!
            .Split('.')
            .Last()
            .Replace('+', '.');
}