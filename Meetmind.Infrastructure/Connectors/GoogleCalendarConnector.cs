using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Meetmind.Application.Connectors;
using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Connectors;

public sealed class GoogleCalendarConnector : ICalendarConnector
{
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<GoogleCalendarConnector> _logger;

    public GoogleCalendarConnector(IDateTimeProvider clock, ILogger<GoogleCalendarConnector> logger)
    {
        _clock = clock;
        _logger = logger;
    }

    public string Source => "Google";

    public async Task<List<CalendarMeetingDto>> GetTodayMeetingsAsync(CancellationToken cancellationToken)
    {
        var service = await GetCalendarServiceAsync(cancellationToken);

        var now = _clock.UtcNow;
        var startOfDay = now.Date;
        var endOfDay = startOfDay.AddDays(1);

        var request = service.Events.List("primary");
        request.TimeMinDateTimeOffset = startOfDay;
        request.TimeMaxDateTimeOffset = endOfDay;
        request.ShowDeleted = false;
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        var result = new List<CalendarMeetingDto>();
        var events = await request.ExecuteAsync(cancellationToken);

        if (events.Items == null) return result;

        foreach (var ev in events.Items)
        {
            if (ev.Start?.DateTime == null) continue;

            result.Add(new CalendarMeetingDto
            {
                ExternalId = ev.Id,
                Source = Source,
                Title = ev.Summary ?? "(Sans titre)",
                StartUtc = ev.Start.DateTime.Value.ToUniversalTime(),
                EndUtc = ev.End?.DateTime?.ToUniversalTime(),
                OrganizerEmail = ev.Organizer?.Email,
                AttendeesEmails = ev.Attendees?.Select(a => a.Email).Where(e => !string.IsNullOrEmpty(e)).ToList()
            });
        }

        return result;
    }
    private async Task<CalendarService> GetCalendarServiceAsync(CancellationToken cancellationToken)
    {
        var credentialsPath = Path.Combine("Resources", "google-credentials.json");
        if (!File.Exists(credentialsPath))
        {
            _logger.LogError("Le fichier de configuration Outlook n'existe pas : {Path}", credentialsPath);
            return new CalendarService();
        }
        using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);

        var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets,
            new[] { CalendarService.Scope.CalendarReadonly },
            "default_user",
            cancellationToken,
            new FileDataStore("GoogleOAuthToken", true)
        );

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MeetMind"
        });
    }

    public async Task<bool> IsCancelledAsync(string externalId, CancellationToken token)
    {
        var service = await GetCalendarServiceAsync(token);
        try
        {
            var calendarId = "primary";
            var ev = await service.Events.Get(calendarId, externalId).ExecuteAsync(token);

            return ev.Status?.ToLower() == "cancelled";
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return true;
        }
    }
}
