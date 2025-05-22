
namespace Meetmind.Application.Services;

public interface ISummaryService
{
    Task<string> GenerateSummaryAsync(Guid meetingId, CancellationToken ct);
}