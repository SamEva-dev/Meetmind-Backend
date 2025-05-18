using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

//public record Language(string Iso639_1);

public sealed class SettingsEntity : AggregateRoot
{
    private SettingsEntity() { }
    public string Language { get; set; }
    public bool AutoStartRecord { get; set; } = false;
    public bool AutoTranscript { get; set; } = false;
    public bool AutoSummarize { get; set; } = false;
    public bool AutoTranslate { get; set; } = false;
    public int NotifyBeforeMinutes { get; set; }
    public int NotificationRepeatInterval { get; set; }
    public bool RequireConsent { get; set; } = true;
    public bool UseGoogleCalendar { get; set; }
    public bool UseOutlookCalendar { get; set; }
    public int RetentionDays { get; set; }

}