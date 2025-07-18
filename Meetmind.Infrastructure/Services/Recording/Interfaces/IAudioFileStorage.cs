
namespace Meetmind.Infrastructure.Services.Recording.Interfaces;

public interface IAudioFileStorage
{
    string GetNewFragmentPath(string meetingName, Guid meetingId);
    Task<string> ConcatenateAsync(IEnumerable<string> fragments, CancellationToken ct);
    void Delete(IEnumerable<string> files);
}