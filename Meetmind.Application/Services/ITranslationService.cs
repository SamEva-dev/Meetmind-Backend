
namespace Meetmind.Application.Services;

public interface ITranslationService
{
    Task<string> TranslateAsync(Guid meetingId, string lang, CancellationToken ct);
}
