using LoanApplication.Domain.Entities;

namespace LoanApplication.Application.Common.Contracts;

public static class Jwt
{
    public record GenerateTokenRequest(User User, IList<string> Roles);

    public record GenerateTokenResponse(string AccessToken, int ExpireMinutes, string RefreshToken);
}

public static class Authentication
{
    public record CheckPasswordRequest(
        Domain.Entities.User User,
        string Password);
}