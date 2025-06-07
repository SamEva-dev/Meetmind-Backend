
namespace Meetmind.Application.Services
{
    public interface ILiveTranscriptionService
    {
        Task TranscribeAndStoreAsync(Guid meetingId, string fragmentPath, CancellationToken ct);
    }
}
