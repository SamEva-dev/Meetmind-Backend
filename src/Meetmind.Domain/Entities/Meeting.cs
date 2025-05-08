using Meetmind.Domain.Enums;

namespace Meetmind.Domain.Entities;

public class Meeting
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime? EndUtc { get; private set; }
    public MeetingState State { get; private set; } = MeetingState.Pending;

    public TranscriptState TranscriptState { get; private set; } = TranscriptState.NotRequested;
    public string? TranscriptPath { get; private set; }

    public TimeSpan? Duration =>
        EndUtc.HasValue ? EndUtc.Value - StartUtc : null;

    private Meeting() { } // EF

    public Meeting(string title, DateTime startUtc)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        Title = title;
        StartUtc = startUtc;
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
}