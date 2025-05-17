using Meetmind.Application.Services;
using Meetmind.Application.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Workers;

public sealed class CalendarWorker : BackgroundService, ICalendarWorker
{
    private readonly ILogger<CalendarWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDateTimeProvider _clock;

    public CalendarWorker(ILogger<CalendarWorker> logger, IServiceScopeFactory scopeFactory, IDateTimeProvider clock)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CalendarWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var meetingCreator = scope.ServiceProvider.GetRequiredService<IMeetingCreatorService>();

                var now = _clock.UtcNow;
                var meetings = await meetingCreator.GetTodayMeetingsFromCalendarsAsync(now, stoppingToken);

                foreach (var calendarMeeting in meetings)
                {
                    var created = await meetingCreator.CreateMeetingIfNotExistsAsync(calendarMeeting, stoppingToken);
                    if (created)
                    {
                        // notifier immédiatement le frontend de la nouvelle réunion
                        await meetingCreator.NotifyMeetingCreatedAsync(calendarMeeting);
                    }
                }

                // Gérer les alertes à 10min / 5min / toutes les 1min
                await meetingCreator.NotifyImminentMeetingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans CalendarWorker : {Message}", ex.Message);
            }
            
        }
    }
}
