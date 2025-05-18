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
using Meetmind.Domain.Models;


namespace Meetmind.Infrastructure.Connectors;


public sealed class OutlookCalendarConnector : ICalendarConnector
{
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<OutlookCalendarConnector> _logger;
    private readonly IOutlookTokenService _tokenService;

    private const string TokenPath = "Resources/outlook-token.json";

    public OutlookCalendarConnector(IDateTimeProvider clock,
        IOutlookTokenService tokenService,
        ILogger<OutlookCalendarConnector> logger)
    {
        _clock = clock;
        _logger = logger;
        _tokenService = tokenService;
    }

    public string Source => "Outlook";

    public async Task<List<CalendarMeetingDto>> GetTodayMeetingsAsync(CancellationToken token)
    {
        var graphClient = await GetGraphClientAsync(token);
        if (graphClient == null)
        {
            _logger.LogError("Impossible de créer le client Graph");
            return new List<CalendarMeetingDto>();
        }

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

    private async Task<GraphServiceClient> GetGraphClientAsync(CancellationToken token)
    {
        var credentialsPath = Path.Combine("Resources", "outlook-credentials.json");
        if (!File.Exists(credentialsPath))
        {
            _logger.LogError("Le fichier de configuration Outlook n'existe pas : {Path}", credentialsPath);
            return null;
        }
        return null;
        var config = JsonSerializer.Deserialize<OutlookCredentials>(
            await File.ReadAllTextAsync(credentialsPath, token))!;

        var cachedCredential = _tokenService.CreateCachedCredential(config);

        return new GraphServiceClient(cachedCredential, config.Scopes);
    }

    public async Task<bool> IsCancelledAsync(string externalId, CancellationToken token)
    {
        var graphClient = await GetGraphClientAsync(token);

        try
        {
            var ev = await graphClient.Me.Events[externalId].GetAsync();
            return ev?.IsCancelled == true;
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            return true;
        }
        catch
        {
            return false;
        }
    }
}