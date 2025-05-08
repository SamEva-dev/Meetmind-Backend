using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Queries;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests;

public class GetTranscriptQueryHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly ILogger<GetTranscriptQueryHandler> _logger = Substitute.For<ILogger<GetTranscriptQueryHandler>>();

    [Fact]
    public async Task Should_Return_Transcript_Content()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var transcriptPath = $"Data/Transcript/{meetingId}.txt";

        Directory.CreateDirectory("Data/Transcript");
        await File.WriteAllTextAsync(transcriptPath, "Hello transcript");

        var meeting = new Meeting("Test transcript", DateTime.UtcNow);
        meeting.QueueTranscription();
        meeting.MarkTranscriptionProcessing();
        meeting.MarkTranscriptionCompleted(transcriptPath);

        _repo.GetByIdAsync(meetingId, Arg.Any<CancellationToken>()).Returns(meeting);
        var handler = new GetTranscriptQueryHandler(_repo, _logger);

        // Act
        var result = await handler.Handle(new GetTranscriptQuery(meetingId), default);

        // Assert
        result.Should().Contain("Hello transcript");
    }

    [Fact]
    public async Task Should_Throw_If_Transcript_Not_Ready()
    {
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting("Pending", DateTime.UtcNow);
        _repo.GetByIdAsync(meetingId, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new GetTranscriptQueryHandler(_repo, _logger);
        var act = async () => await handler.Handle(new GetTranscriptQuery(meetingId), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }
}