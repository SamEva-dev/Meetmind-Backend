using Azure.Core;
using Azure.Identity;
using Meetmind.Application.Services;
using Meetmind.Domain.Models;
using Meetmind.Infrastructure.Helper;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services;

public class OutlookTokenService : IOutlookTokenService
{
    private readonly IHubContext<MeetingHub> _hubContext;
    private readonly ILogger<OutlookTokenService> _logger;

    public OutlookTokenService(IHubContext<MeetingHub> hubContext, ILogger<OutlookTokenService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public TokenCredential CreateCachedCredential(OutlookCredentials config)
    {
        var options = new DeviceCodeCredentialOptions
        {
            ClientId = config.ClientId,
            TenantId = config.TenantId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            DeviceCodeCallback = async(code, cancellation) =>
            {
                _logger.LogInformation("Device code: {Msg}", code.Message);

                await _hubContext.Clients.All.SendAsync("DeviceLoginRequested", new
                {
                    Url = code.VerificationUri.ToString(),
                    Code = code.UserCode,
                    ExpiresOn = code.ExpiresOn,
                    Message = code.Message
                });
            }
        };

        var credential = new DeviceCodeCredential(options);
        return new CachedTokenCredential(credential, config.Scopes, "Resources/outlook-token.json");
    }
}
