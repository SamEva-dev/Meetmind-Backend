using Meetmind.Domain.Entities;

namespace Meetmind.Application.Services;

public interface ISummarizeService
{
    Task<string> SummarizeAsync(MeetingEntity meeting, CancellationToken ct);
}
