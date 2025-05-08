using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Meetmind.Infrastructure.Transcription;

public class TranscriptSemanticAnalyzer
{
    public async Task<string> AnalyzeAsync(Guid meetingId, string transcriptPath, CancellationToken ct)
    {
        if (!File.Exists(transcriptPath))
            throw new FileNotFoundException("Transcript missing", transcriptPath);

        var lines = await File.ReadAllLinesAsync(transcriptPath, ct);
        var participants = new Dictionary<string, int>(); // speaker → count
        var timeline = new List<object>();
        var keywords = new Dictionary<string, int>();

        var timePattern = new Regex(@"^\[(\d{2}:\d{2}:\d{2})\] (.+?): (.+)$"); // [00:05:12] Speaker: message

        foreach (var line in lines)
        {
            var match = timePattern.Match(line);
            if (!match.Success) continue;

            var time = match.Groups[1].Value;
            var speaker = match.Groups[2].Value.Trim();
            var text = match.Groups[3].Value.Trim();

            if (!participants.ContainsKey(speaker))
                participants[speaker] = 0;
            participants[speaker] += 1;

            timeline.Add(new { time, speaker, text });

            foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var clean = word.Trim().ToLowerInvariant();
                if (clean.Length < 4 || clean.Length > 20 || clean.StartsWith('[')) continue;
                keywords[clean] = keywords.GetValueOrDefault(clean, 0) + 1;
            }
        }

        var output = new
        {
            meetingId,
            dateUtc = DateTime.UtcNow,
            participants = participants
                .Select(p => new { id = p.Key.Replace(" ", "_").ToLower(), label = p.Key, speakingTurns = p.Value }),
            keywords = keywords.OrderByDescending(k => k.Value).Take(10).Select(k => k.Key).ToList(),
            timeline
        };

        var json = JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var outputPath = Path.Combine("Data", "Transcript", $"{meetingId}.analysis.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, json, ct);

        return outputPath;
    }
}