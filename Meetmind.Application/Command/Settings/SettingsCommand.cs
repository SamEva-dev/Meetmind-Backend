
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Domain.Enums;

namespace Meetmind.Application.Command.Settings;

public record SettingsCommand: IRequest<SettingsDto>
{
    public LanguageCode Language { get; set; }
    public bool AutoStartRecord { get; set; } = false;
    public bool AutoStopRecord { get; set; } = false;
    public bool AutoTranscript { get; set; } = false;
    public bool AutoSummarize { get; set; } = false;
    public bool AutoTranslate { get; set; } = false;
    public int NotifyBeforeMinutes { get; set; }
    public int NotificationRepeatInterval { get; set; }
    public bool RequireConsent { get; set; } = true;
    public int RetentionDays { get; set; }
    public bool UseGoogleCalendar { get; set; }
    public bool UseOutlookCalendar { get; set; }
    public bool AutoCancelMeeting { get; set; }
    public bool AutoDeleteMeeting { get; set; }
    public TranscriptionType TranscriptionType { get; set; }
    public AudioRecordingType AudioRecordingType { get; set; }
    public WhisperModelType WhisperModelType { get; set; }
    public WhisperDeviceType WhisperDeviceType { get; set; }
    public WhisperComputeType WhisperComputeType { get; set; }
    public DiarizationModelType DiarizationModelType { get; set; }
}

