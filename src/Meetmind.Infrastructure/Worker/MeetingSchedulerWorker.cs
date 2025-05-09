using Meetmind.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Worker;

public class MeetingSchedulerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MeetingSchedulerWorker> _logger;
    private readonly List<int> _notifyMinutes = [10, 5, 1];

    public MeetingSchedulerWorker(IServiceScopeFactory scopeFactory, ILogger<MeetingSchedulerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MeetingSchedulerWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var calendar = scope.ServiceProvider.GetRequiredService<ICalendarService>();
                var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Simule l'identité actuelle
                var meetings = await calendar.GetTodayMeetingsAsync("user-token", stoppingToken);

                var now = DateTime.UtcNow;

                foreach (var meeting in meetings)
                {
                    var diff = (int)(meeting.StartUtc - now).TotalMinutes;

                    foreach (var notifyAt in _notifyMinutes)
                    {
                        if (diff == notifyAt)
                        {
                            _logger.LogInformation("Notifying meeting {Id} ({Title}) in {Min} min", meeting.Id, meeting.Title, notifyAt);
                            await notifier.NotifyUpcomingAsync(meeting, notifyAt, stoppingToken);
                        }
                    }

                    if (diff == 0)
                    {
                        _logger.LogInformation("Meeting {Id} started. Asking UI to confirm action", meeting.Id);
                        await notifier.RequestUserConfirmationAsync(meeting.Id, "start_record", stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MeetingSchedulerWorker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}