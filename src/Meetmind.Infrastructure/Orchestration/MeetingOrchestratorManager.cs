using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Enums;
using Meetmind.Domain.Models.Realtime;
using Meetmind.Domain.Models;
using Meetmind.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Orchestration;

public class MeetingOrchestratorManager : IMeetingOrchestratorManager
{
    private readonly ILogger<MeetingOrchestratorManager> _logger;
    private readonly INotificationService _notifier;

    private readonly Dictionary<Guid, MeetingOrchestrator> _active = new();

    public MeetingOrchestratorManager(ILogger<MeetingOrchestratorManager> logger, INotificationService notifier)
    {
        _logger = logger;
        _notifier = notifier;
    }

    public async Task StartOrchestratorAsync(UpcomingMeeting meeting, UserSettings settings, CancellationToken ct)
    {
        if (_active.ContainsKey(meeting.Id))
        {
            _logger.LogWarning("Orchestrator already exists for meeting {Id}", meeting.Id);
            return;
        }

        var orchestrator = new MeetingOrchestrator(meeting, settings, _notifier,
            _loggerFactory.CreateLogger<MeetingOrchestrator>());

        _active[meeting.Id] = orchestrator;

        _logger.LogInformation("Started orchestrator for meeting {Id}", meeting.Id);

        // Lance la logique de démarrage (notify + actions)
        await orchestrator.HandleStartAsync(ct);
    }

    public async Task HandleConfirmationAsync(ConfirmActionMessage message, CancellationToken ct)
    {
        if (_active.TryGetValue(message.MeetingId, out var orchestrator))
        {
            await orchestrator.HandleUserConfirmation(message, ct);
        }
        else
        {
            _logger.LogWarning("No orchestrator found for Meeting {Id}", message.MeetingId);
        }
    }

    public MeetingExecutionState? GetMeetingState(Guid meetingId)
    {
        return _active.TryGetValue(meetingId, out var o) ? o.State : null;
    }

    private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
}