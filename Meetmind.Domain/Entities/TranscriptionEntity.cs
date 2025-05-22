using System.Text.Json;
using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

public class TranscriptionEntity : AggregateRoot
{
    public Guid MeetingId { get; set; }
    public string Tilte { get; set; }
    public string SourceFile { get; set; }
    public string Text { get; set; }
    public string Language { get; set; }
    public double? LanguageProbability { get; set; }
    public double? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Speakers { get; set; } = new();

    public ICollection<TranscriptionSegment> Segments { get; set; }
}

public class TranscriptionSegment : AggregateRoot
{
    public Guid TranscriptionId { get; set; }
    public string Speaker { get; set; }
    public string Text { get; set; }
    public string Start { get; set; } // HH:mm:ss.SSS
    public string End { get; set; }   // HH:mm:ss.SSS

    public TranscriptionEntity Transcription { get; set; }
}