
namespace Meetmind.Domain.ValueObjects;

public class UserSettings
{
    public bool AutoStartRecord { get; set; } = false;
    public bool AutoTranscript { get; set; } = false;
    public bool AutoSummarize { get; set; } = false;
    public bool AutoTranslate { get; set; } = false;

    // Temps d'alerte avant réunion (en minutes)
    public List<int> NotifyBeforeMinutes { get; set; } = new() { 10, 5, 1 };
}