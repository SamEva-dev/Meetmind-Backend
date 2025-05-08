using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;

namespace MeetingTests;

public class MeetingTranscriptionTests
{
    [Fact]
    public void Can_Queue_Transcription_When_NotRequested()
    {
        var meeting = new Meeting("With transcript", DateTime.UtcNow);
        meeting.QueueTranscription();

        meeting.TranscriptState.Should().Be(TranscriptState.Queued);
    }

    [Fact]
    public void Cannot_Queue_Twice()
    {
        var meeting = new Meeting("Double queue", DateTime.UtcNow);
        meeting.QueueTranscription();

        var act = () => meeting.QueueTranscription();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Can_Mark_As_Processing_After_Queued()
    {
        var meeting = new Meeting("To process", DateTime.UtcNow);
        meeting.QueueTranscription();
        meeting.MarkTranscriptionProcessing();

        meeting.TranscriptState.Should().Be(TranscriptState.Processing);
    }

    [Fact]
    public void Can_Mark_As_Completed_With_Path()
    {
        var meeting = new Meeting("Done", DateTime.UtcNow);
        meeting.QueueTranscription();
        meeting.MarkTranscriptionProcessing();
        meeting.MarkTranscriptionCompleted("Data/Transcript/test.txt");

        meeting.TranscriptState.Should().Be(TranscriptState.Completed);
        meeting.TranscriptPath.Should().Be("Data/Transcript/test.txt");
    }

    [Fact]
    public void Can_Mark_As_Failed()
    {
        var meeting = new Meeting("Fail test", DateTime.UtcNow);
        meeting.QueueTranscription();
        meeting.MarkTranscriptionFailed();

        meeting.TranscriptState.Should().Be(TranscriptState.Failed);
    }
}