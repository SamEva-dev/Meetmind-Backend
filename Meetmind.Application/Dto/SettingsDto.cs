
namespace Meetmind.Application.Dto
{
    public class SettingsDto
    {
        public string Language { get; set; }
        public bool AutoStartRecord { get; set; } = false;
        public bool AutoTranscript { get; set; } = false;
        public bool AutoSummarize { get; set; } = false;
        public bool AutoTranslate { get; set; } = false;
        public int NotifyBeforeMinutes { get; set; }
        public int NotificationRepeatInterval { get; set; }
        public bool RequireConsent { get; set; } = true;
        public int RetentionDays { get; set; }
    }
}
