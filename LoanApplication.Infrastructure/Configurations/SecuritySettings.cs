namespace LoanApplication.Infrastructure.Configurations;

internal sealed record SecuritySettings
{
    public const string Path = "Security";
    public string ClientId { get; set; } = string.Empty;
}

internal sealed record AuthSettings
{
    public const string Path = "Security:Authentication";
    public int MaxFailedAttempts { get; set; }
    public int BaseLockoutMinutes { get; set; }
    public int LockoutMultiplier { get; set; }
    public int MaxLockoutMinutes { get; set; }
}

#region Jwt Settings

internal sealed record JwtSettings
{
    public const string Path = "Security:Jwt";
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string PrivateKey { get; init; } = string.Empty;
    public string PublicKey { get; init; } = string.Empty;
    public int ExpireMinutes { get; init; }
    public int RefreshTokenExpireDays { get; init; }
}

#endregion

#region Encryption Settings

internal sealed record EncryptionSettings
{
    public const string Path = "Security:Encryption";

    public Key[] Keys { get; set; } = [];
    public string Algorithm { get; set; } = string.Empty;
};

#endregion

#region Hashing Settings

internal sealed record HashingSettings
{
    public const string Path = "Security:Hashing";

    public Key[] Keys { get; set; } = [];
    public string Algorithm { get; set; } = string.Empty;
}

#endregion

internal sealed record Key
{
    public string KeyId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}