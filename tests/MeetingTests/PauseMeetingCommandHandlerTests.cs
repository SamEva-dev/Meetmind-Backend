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
public class PauseMeetingCommandHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Should_Pause_Meeting()
    {
        var meeting = new Meeting("Meeting", DateTime.UtcNow);
        meeting.Start();
        _repo.GetByIdAsync(meeting.Id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new PauseMeetingCommandHandler(_repo, _uow);
        await handler.Handle(new PauseMeetingCommand(meeting.Id), default);

        meeting.State.Should().Be(MeetingState.Paused);
    }
}
