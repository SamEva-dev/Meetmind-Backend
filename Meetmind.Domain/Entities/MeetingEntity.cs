using Meetmind.Domain.Enums;
using Meetmind.Domain.Events;
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

    public DateTime Start { get; private set; }
    public DateTime? End { get; private set; }

    public MeetingState State { get; private set; } = MeetingState.Pending;

    public TranscriptState TranscriptState { get; private set; } = TranscriptState.NotRequested;
    public string? TranscriptPath { get; private set; }

    public SummaryState SummaryState { get; private set; } = SummaryState.NotRequested;
    public string? SummaryPath { get; private set; }
    public bool IsCancelled { get; private set; }

    public string? AudioPath { get; private set; }


    public TimeSpan? Duration =>
        EndUtc.HasValue ? EndUtc.Value - StartUtc : null;

    private MeetingEntity() { }

    public MeetingEntity(string title, DateTime start, DateTime? ends)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
        Title = title;
        Start = start;
        End = ends; 
        StartUtc = start.ToUniversalTime();
        EndUtc = ends?.ToUniversalTime();
    }

    public MeetingEntity(string title, DateTime start, DateTime? end, string externalId, string source) : this(title, start, end)
    {
        ExternalId = externalId;
        ExternalSource = source;
    }

    public void MakePending()
    {
        State = MeetingState.Pending;
    }

    public void MakeRequested()
    {
        TranscriptState = TranscriptState.NotRequested;
    }

    public void StartRecording()
    {
        if (State != MeetingState.Pending)
            throw new InvalidOperationException("Meeting already started.");
        State = MeetingState.Recording;
        AddDomainEvent(new MeetingStartedDomainEvent(Id));
    }

    public void PauseRecording()
    {
        if (State != MeetingState.Recording)
            throw new InvalidOperationException("Cannot pause unless recording.");
        State = MeetingState.Paused;
        AddDomainEvent(new MeetingPausedDomainEvent(Id));
    }

    public void ResumeRecording()
    {
        if (State != MeetingState.Paused)
            throw new InvalidOperationException("Can only resume from pause.");
        State = MeetingState.Recording;
        AddDomainEvent(new MeetingResumedDomainEvent(Id));
    }

    public void StopRecording(DateTime endUtc)
    {
        if (State is not MeetingState.Recording and not MeetingState.Paused)
            throw new InvalidOperationException("Can only stop active meeting.");
        EndUtc = endUtc;
        State = MeetingState.Done;
        AddDomainEvent(new MeetingStoppedDomainEvent(Id));
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

    public void MarkTranscriptionCompleted()
    {
        if (TranscriptState != TranscriptState.Processing)
            throw new InvalidOperationException("Transcription must be processing before completed.");
        if (State is not  MeetingState.Done)
            throw new InvalidOperationException("Transcription can only be completed for done meetings.");
        TranscriptState = TranscriptState.Completed;
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

    public void Cancel()
    {
        if (IsCancelled || State == MeetingState.Done)
            return;

        IsCancelled = true;
        State = MeetingState.Cancelled;
    }

    public void AttachAudio(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("AudioPath is required");

        AudioPath = path;
    }

    public void DetachAudio()
    {
        AudioPath = null;
    }

}