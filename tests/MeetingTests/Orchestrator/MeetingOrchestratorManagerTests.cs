using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Enums;
using Meetmind.Domain.Models.Realtime;
using Meetmind.Domain.Models;
using Meetmind.Domain.ValueObjects;
using Meetmind.Infrastructure.Orchestration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests.Orchestrator;

public class MeetingOrchestratorManagerTests
{
    private readonly INotificationService _notifier = Substitute.For<INotificationService>();
    private readonly ILogger<MeetingOrchestratorManager> _logger = Substitute.For<ILogger<MeetingOrchestratorManager>>();

    [Fact]
    public async Task Should_Start_And_Track_Orchestrator()
    {
        var manager = new MeetingOrchestratorManager(_logger, _notifier);
        var meeting = new UpcomingMeeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddMinutes(30),
            Source = "test"
        };

        var settings = new UserSettings { AutoStartRecord = true };

        await manager.StartOrchestratorAsync(meeting, settings, CancellationToken.None);

        var state = manager.GetMeetingState(meeting.Id);
        state.Should().Be(MeetingExecutionState.Recording);
    }

    [Fact]
    public async Task Should_Not_Create_Duplicate_Orchestrator()
    {
        var manager = new MeetingOrchestratorManager(_logger, _notifier);
        var id = Guid.NewGuid();
        var meeting = new UpcomingMeeting
        {
            Id = id,
            Title = "Unique",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddMinutes(30),
            Source = "test"
        };

        var settings = new UserSettings { AutoStartRecord = true };

        await manager.StartOrchestratorAsync(meeting, settings, CancellationToken.None);
        await manager.StartOrchestratorAsync(meeting, settings, CancellationToken.None); // duplicate call

        var state = manager.GetMeetingState(id);
        state.Should().Be(MeetingExecutionState.Recording); // still valid, not reset
    }

    [Fact]
    public async Task Should_Handle_User_Confirmation()
    {
        var manager = new MeetingOrchestratorManager(_logger, _notifier);
        var meetingId = Guid.NewGuid();
        var meeting = new UpcomingMeeting
        {
            Id = meetingId,
            Title = "Confirm Test",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddMinutes(30),
            Source = "test"
        };

        var settings = new UserSettings { AutoStartRecord = false };
        await manager.StartOrchestratorAsync(meeting, settings, CancellationToken.None);

        // simulate user confirmation
        var confirmation = new ConfirmActionMessage
        {
            MeetingId = meetingId,
            Action = "start_record",
            Accepted = true
        };

        await manager.HandleConfirmationAsync(confirmation, CancellationToken.None);

        var state = manager.GetMeetingState(meetingId);
        state.Should().Be(MeetingExecutionState.Recording);
    }
}