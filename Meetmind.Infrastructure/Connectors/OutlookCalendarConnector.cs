using System.Text.Json;
using Meetmind.Application.Connectors;
using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Azure.Identity;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace Meetmind.Infrastructure.Connectors;


public sealed class OutlookCalendarConnector : ICalendarConnector
{
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<OutlookCalendarConnector> _logger;
    private readonly IHubContext<MeetingHub> _hub;
    private const string TokenPath = "Resources/outlook-token.json";

    public OutlookCalendarConnector(IDateTimeProvider clock,
        IHubContext<MeetingHub> hub,
        ILogger<OutlookCalendarConnector> logger)
    {
        _clock = clock;
        _logger = logger;
        _hub = hub;
    }

    public async Task<List<CalendarMeetingDto>> GetTodayMeetingsAsync(CancellationToken token)
    {
        var credentialsPath = Path.Combine("Resources", "outlook-credentials.json");
        if (!File.Exists(credentialsPath))
        {
            _logger.LogError("Le fichier de configuration Outlook n'existe pas : {Path}", credentialsPath);
            return new List<CalendarMeetingDto>();
        }

        var config = JsonSerializer.Deserialize<OutlookCredentials>(
            await File.ReadAllTextAsync(credentialsPath, token))!;

        var options = new DeviceCodeCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            ClientId = config.ClientId,
            TenantId = config.TenantId,
            DeviceCodeCallback = (code, cancellation) =>
            {
                Console.WriteLine(code.Message);
                NotifyConfirmAccesAsync(new
                {
                    Code = code.UserCode,
                    Url = code.VerificationUri,
                    Message = code.Message
                }, cancellation);
                _logger.LogInformation(code.Message);
                return Task.FromResult(0);
            },
        };
        var deviceCodeCredential = new DeviceCodeCredential(options);

        var graphClient = new GraphServiceClient(deviceCodeCredential, config.Scopes);

        var start = _clock.UtcNow.Date;
        var end = start.AddDays(1);

        var events = await graphClient.Me.CalendarView
            .GetAsync(config =>
            {
                config.QueryParameters.StartDateTime = start.ToString("o");
                config.QueryParameters.EndDateTime = end.ToString("o");
                config.Headers.Add("Prefer", "outlook.timezone=\"UTC\"");
            }, token);

        var result = new List<CalendarMeetingDto>();

        if (events?.Value == null) return result;

        foreach (var ev in events.Value)
        {
            if (ev.Start?.DateTime == null) continue;

            result.Add(new CalendarMeetingDto
            {
                ExternalId = ev.Id,
                Source = "Outlook",
                Title = ev.Subject ?? "(Sans titre)",
                StartUtc = DateTime.Parse(ev.Start.DateTime).ToUniversalTime(),
                EndUtc = ev.End?.DateTime != null ? DateTime.Parse(ev.End.DateTime).ToUniversalTime() : null,
                OrganizerEmail = ev.Organizer?.EmailAddress?.Address,
                AttendeesEmails = ev.Attendees?
                    .Select(a => a.EmailAddress?.Address)
                    .Where(addr => !string.IsNullOrWhiteSpace(addr))
                    .ToList()
            });
        }

        return result;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken token)
    {
        var credentialsPath = Path.Combine("Resources", "google-credentials.json");

        var config = JsonSerializer.Deserialize<OutlookCredentials>(
            await File.ReadAllTextAsync(credentialsPath, token))!;

        var app = PublicClientApplicationBuilder.Create(config.ClientId)
            .WithRedirectUri(config.RedirectUri)
            .WithTenantId(config.TenantId)
            .Build();

        var scopes = config.Scopes;

        AuthenticationResult result;

        if (File.Exists(TokenPath))
        {
            var accounts = await app.GetAccountsAsync();
            try
            {
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync(token);
                return result.AccessToken;
            }
            catch
            {
                // token expiré, on retente interactif
            }
        }

        // Flow interactif
        result = await app.AcquireTokenInteractive(scopes).ExecuteAsync(token);

        await File.WriteAllTextAsync(TokenPath, JsonSerializer.Serialize(new
        {
            result.AccessToken,
            result.ExpiresOn
        }), token);

        return result.AccessToken;
    }

    public async Task NotifyConfirmAccesAsync(object dto, CancellationToken token)
    {
        await _hub.Clients.All.SendAsync("OutlookAuthCode", dto);
    }

    private sealed class OutlookCredentials
    {
        public string ClientId { get; set; } = default!;
        public string TenantId { get; set; } = default!;
        public string RedirectUri { get; set; } = default!;
        public string[] Scopes { get; set; } = default!;
    }
}