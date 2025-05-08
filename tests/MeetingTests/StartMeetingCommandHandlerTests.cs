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

public class StartMeetingCommandHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Should_Start_Meeting()
    {
        var meeting = new Meeting("Test Meeting", DateTime.UtcNow);
        _repo.GetByIdAsync(meeting.Id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new StartMeetingCommandHandler(_repo, _uow);
        await handler.Handle(new StartMeetingCommand(meeting.Id), default);

        meeting.State.Should().Be(MeetingState.Recording);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_If_Not_Found()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Meeting?)null);

        var handler = new StartMeetingCommandHandler(_repo, _uow);
        var act = async () => await handler.Handle(new StartMeetingCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}