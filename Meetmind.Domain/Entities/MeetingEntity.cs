using Meetmind.Domain.Enums;
using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

public class MeetingEntity : AggregateRoot
{
    private string source; 

    public string Title { get; private set; }
     public string? ExternalId { get; private set; }
    public string? ExternalSource { get; private set; }
    public DateTime StartUtc { get;  set; }
    public DateTime? EndUtc { get; private set; }
    public MeetingState State { get; private set; } = MeetingState.Pending;

    public TranscriptState TranscriptState { get; private set; } = TranscriptState.NotRequested;
    public string? TranscriptPath { get; private set; }

    public SummaryState SummaryState { get; private set; } = SummaryState.NotRequested;
    public string? SummaryPath { get; private set; }

    public TimeSpan? Duration =>
        EndUtc.HasValue ? EndUtc.Value - StartUtc : null;

    private MeetingEntity() { }

    public MeetingEntity(string title, DateTime startUtc)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        Title = title;
        StartUtc = startUtc;
    }

    public MeetingEntity(string title, DateTime startUtc, string externalId, string source) : this(title, startUtc)
    {
        ExternalId = externalId;
        ExternalSource = source;
    }

    public void Start()
    {
        if (State != MeetingState.Pending)
            throw new InvalidOperationException("Meeting already started.");
        State = MeetingState.Recording;
    }

    public void Pause()
    {
        if (State != MeetingState.Recording)
            throw new InvalidOperationException("Cannot pause unless recording.");
        State = MeetingState.Paused;
    }

    public void Resume()
    {
        if (State != MeetingState.Paused)
            throw new InvalidOperationException("Can only resume from pause.");
        State = MeetingState.Recording;
    }

    public void Stop(DateTime endUtc)
    {
        if (State is not MeetingState.Recording and not MeetingState.Paused)
            throw new InvalidOperationException("Can only stop active meeting.");
        EndUtc = endUtc;
        State = MeetingState.Done;
    }

    public void QueueTranscription()
    {
        if (TranscriptState != TranscriptState.NotRequested)
            throw new InvalidOperationException("Transcription already queued or processed.");
        TranscriptState = TranscriptState.Queued;
    }

    public void MarkTranscriptionProcessing()
    {
        if (TranscriptState != TranscriptState.Queued)
            throw new InvalidOperationException("Transcription must be queued before processing.");
        TranscriptState = TranscriptState.Processing;
    }

    public void MarkTranscriptionCompleted(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Transcript path cannot be empty.");

        TranscriptState = TranscriptState.Completed;
        TranscriptPath = path;
    }

    public void MarkTranscriptionFailed()
    {
        TranscriptState = TranscriptState.Failed;
    }

    public void QueueSummary()
    {
        if (SummaryState != SummaryState.NotRequested)
            throw new InvalidOperationException("Summary already requested.");
        SummaryState = SummaryState.Queued;
    }

    public void MarkSummaryProcessing()
    {
        if (SummaryState != SummaryState.Queued)
            throw new InvalidOperationException("Must be queued before processing.");
        SummaryState = SummaryState.Processing;
    }

    public void MarkSummaryCompleted(string path)
    {
        SummaryState = SummaryState.Completed;
        SummaryPath = path;
    }

    public void MarkSummaryFailed()
    {
        SummaryState = SummaryState.Failed;
    }
}