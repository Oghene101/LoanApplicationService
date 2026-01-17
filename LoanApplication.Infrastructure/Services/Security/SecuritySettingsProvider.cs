using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace LoanApplication.Infrastructure.Services.Security;

internal sealed class SecuritySettingsProvider(
    IOptions<SecuritySettings> security) : ISecuritySettingsProvider
{
    private readonly SecuritySettings _securitySettings = security.Value;

    public string GetClientId()
        => _securitySettings.ClientId;
}