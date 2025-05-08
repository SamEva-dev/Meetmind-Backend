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
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests;

public class TriggerTranscriptionCommandHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<TriggerTranscriptionCommandHandler> _log =
        Substitute.For<ILogger<TriggerTranscriptionCommandHandler>>();

    [Fact]
    public async Task Should_Queue_Transcription()
    {
        var meeting = new Meeting("To transcribe", DateTime.UtcNow);
        _repo.GetByIdAsync(meeting.Id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new TriggerTranscriptionCommandHandler(_repo, _uow, _log);

        await handler.Handle(new TriggerTranscriptionCommand(meeting.Id), default);

        meeting.TranscriptState.Should().Be(TranscriptState.Queued);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}