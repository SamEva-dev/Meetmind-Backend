using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Domain.Enums;

namespace Meetmind.Domain.Entities;

public class Meeting
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime? EndUtc { get; private set; }
    public MeetingState State { get; private set; } = MeetingState.Pending;

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
}