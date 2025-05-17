using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace Meetmind.Infrastructure.Services;

public sealed class OutlookAuthService : IOutlookAuthService
{
    private readonly IConfiguration _config;

    public OutlookAuthService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken token)
    {
        var clientId = _config["Outlook:ClientId"];
        var tenantId = _config["Outlook:TenantId"];
        var clientSecret = _config["Outlook:ClientSecret"];
        var refreshToken = await File.ReadAllTextAsync("outlook-refresh-token.txt", token); // ⚠️ Pour tests uniquement

        var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .Build();

        // Fix: Replace the invalid 'AcquireTokenByRefreshToken' call with 'AcquireTokenForClient'
        var result = await app.AcquireTokenForClient(
            new[] { "https://graph.microsoft.com/.default" }
        ).ExecuteAsync(token);

        return result.AccessToken;
    }
}
