using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Summary;

public class MarkdownSummaryService : ISummaryService
{
    private readonly ILogger<MarkdownSummaryService> _logger;

    public MarkdownSummaryService(ILogger<MarkdownSummaryService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateSummaryAsync(Guid meetingId, CancellationToken ct)
    {
        var transcriptPath = Path.Combine("Data", "Transcript", $"{meetingId}.txt");
        var summaryPath = Path.Combine("Data", "Summary", $"{meetingId}.md");

        if (!File.Exists(transcriptPath))
        {
            _logger.LogError("Transcript file missing: {Path}", transcriptPath);
            throw new FileNotFoundException("Transcript not found", transcriptPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(summaryPath)!);

        var lines = await File.ReadAllLinesAsync(transcriptPath, ct);

        var actions = lines.Where(l => l.Contains("todo", StringComparison.OrdinalIgnoreCase)).ToList();
        var decisions = lines.Where(l => l.Contains("decided", StringComparison.OrdinalIgnoreCase)).ToList();
        var intro = lines.Take(2).FirstOrDefault() ?? "Meeting summary generated from transcript.";
        var speakers = lines
                        .Select(l => l.TrimStart())
                        .Where(l => l.Contains(":"))
                        .Select(l => l.Split(':', 2)[0].Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();
        var md = $"""
        # 📝 Meeting Summary – {meetingId}

        {intro}

        ## ✅ Decisions
        {string.Join("\n", decisions.Select(d => $"- {d.Trim()}"))}

        ## 📌 Action Items
        {string.Join("\n", actions.Select(a => $"- {a.Trim()}"))}

        ## 🗣️ Participants
        _(auto-extraction coming soon)_
        {string.Join("\n", speakers.Select(s => $"- {s}"))}

        **Generated on:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
        """;

        await File.WriteAllTextAsync(summaryPath, md, ct);

        _logger.LogInformation("Summary saved to {Path}", summaryPath);
        return summaryPath;
    }
}