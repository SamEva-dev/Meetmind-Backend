namespace Meetmind.Application.Common.Interfaces;

public interface IWhisperService
{
    Task<string> TranscribeAsync(Guid meetingId, CancellationToken ct);
}