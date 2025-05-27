using Meetmind.Domain.Enums;
using Meetmind.Domain.Units;

namespace Meetmind.Domain.Entities;

//public record Language(string Iso639_1);

public sealed class SettingsEntity : AggregateRoot
{
    private SettingsEntity() { }
    public LanguageCode Language { get; set; } = LanguageCode.En;
    public bool AutoStartRecord { get; set; } = false;
    public bool AutoStopRecord { get; set; } = false;
    public bool AutoTranscript { get; set; } = false;
    public bool AutoSummarize { get; set; } = false;
    public bool AutoTranslate { get; set; } = false;
    public int NotifyBeforeMinutes { get; set; }
    public int NotificationRepeatInterval { get; set; }
    public bool RequireConsent { get; set; } = true;
    public bool UseGoogleCalendar { get; set; }
    public bool UseOutlookCalendar { get; set; }
    public int RetentionDays { get; set; }
    public bool AutoCancelMeeting { get; set; }
    public bool AutoDeleteMeeting { get; set; }
    public bool AutoCleanOrphanFragments { get; set; }

    public TranscriptionType TranscriptionType { get; set; } = TranscriptionType.Grpc;
    public AudioRecordingType AudioRecordingType { get; set; } = AudioRecordingType.Native;
    public WhisperModelType WhisperModelType { get; set; } = WhisperModelType.Auto;
    public WhisperDeviceType WhisperDeviceType { get; set; } = WhisperDeviceType.Auto;
    public WhisperComputeType WhisperComputeType { get; set; } = WhisperComputeType.Auto;
    public DiarizationModelType DiarizationModelType { get; set; } = DiarizationModelType.Auto;
    public SummaryModelType SummarizeModelType { get; set; } = SummaryModelType.Auto;
    public SummarizeDetailLevel SummarizeDetailLevel { get; set; } = SummarizeDetailLevel.Standard;
}