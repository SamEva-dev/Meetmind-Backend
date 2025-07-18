using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Services.Transcription.Interfaces;

public interface ITranscriptionStrategy
{
    Task<TranscriptionDto> TranscribeAsync(Stream audioStream, SettingsEntity settings, CancellationToken ct);
}
