using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Application.Commands;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using NSubstitute;

namespace MeetingTests;

public class StopMeetingCommandHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Should_Stop_Meeting()
    {
        var meeting = new Meeting("Stopper", DateTime.UtcNow);
        meeting.Start();
        var end = DateTime.UtcNow.AddMinutes(15);

        _repo.GetByIdAsync(meeting.Id, Arg.Any<CancellationToken>()).Returns(meeting);
        var handler = new StopMeetingCommandHandler(_repo, _uow);

        await handler.Handle(new StopMeetingCommand(meeting.Id, end), default);

        meeting.State.Should().Be(MeetingState.Done);
        meeting.EndUtc.Should().Be(end);
    }
}
