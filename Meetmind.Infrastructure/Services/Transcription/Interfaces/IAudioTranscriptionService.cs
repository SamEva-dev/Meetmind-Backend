

using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Services.Transcription.Interfaces;

public interface IAudioTranscriptionService
{
    Task<TranscriptionDto> TranscribeMeetingAsync(MeetingEntity meeting, CancellationToken ct);
    Task TranscribeChunkAsync(Guid meetingId, string chunkPath, SettingsEntity setting, CancellationToken ct);
}