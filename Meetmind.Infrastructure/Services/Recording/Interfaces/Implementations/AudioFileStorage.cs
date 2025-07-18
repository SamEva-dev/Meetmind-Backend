using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces.Implementations;

internal sealed class AudioFileStorage : IAudioFileStorage
{
    public string GetNewFragmentPath(string meetingName, Guid meetingId)
    {
        var directory = Path.Combine("Resources", "audio", meetingId.ToString("N"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{meetingName}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.wav");
    }

    public async Task<string> ConcatenateAsync(IEnumerable<string> fragments, CancellationToken ct)
    {
        var first = fragments.First();
        var output = Path.Combine(Path.GetDirectoryName(first)!, "final.wav");
        var firstFormat = new WaveFileReader(first).WaveFormat;
        await using var writer = new WaveFileWriter(output, firstFormat);
        foreach (var frag in fragments)
        {
            await using var reader = new WaveFileReader(frag);
            await reader.CopyToAsync(writer, ct);
        }
        return output;
    }

    public void Delete(IEnumerable<string> files)
    {
        foreach (var f in files) try { File.Delete(f); } catch { }
    }
}