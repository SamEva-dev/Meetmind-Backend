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

public class ResumeMeetingCommandHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Should_Resume_From_Pause()
    {
        var meeting = new Meeting("Resumable", DateTime.UtcNow);
        meeting.Start();
        meeting.Pause();
        _repo.GetByIdAsync(meeting.Id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new ResumeMeetingCommandHandler(_repo, _uow);
        await handler.Handle(new ResumeMeetingCommand(meeting.Id), default);

        meeting.State.Should().Be(MeetingState.Recording);
    }
}
