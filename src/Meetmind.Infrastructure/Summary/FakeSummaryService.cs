using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;

namespace Meetmind.Infrastructure.Summary;

public class FakeSummaryService : ISummaryService
{
    public async Task<string> GenerateSummaryAsync(Guid meetingId, CancellationToken ct)
    {
        var outputPath = Path.Combine("Data", "Summary", $"{meetingId}.md");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, $"# Summary for {meetingId}", ct);
        return outputPath;
    }
}
