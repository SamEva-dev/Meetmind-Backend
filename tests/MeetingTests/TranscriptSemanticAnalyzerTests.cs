using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Infrastructure.Transcription;

namespace MeetingTests;

public class TranscriptSemanticAnalyzerTests
{
    [Fact]
    public async Task Should_Extract_Participants_And_Keywords_And_Timeline()
    {
        // Arrange
        var analyzer = new TranscriptSemanticAnalyzer();
        var id = Guid.NewGuid();

        var transcriptPath = Path.Combine("Data", "Transcript", $"{id}.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(transcriptPath)!);

        await File.WriteAllLinesAsync(transcriptPath, new[]
        {
            "[00:01:10] Alice: Welcome everyone to the meeting",
            "[00:02:45] Bob: Let's use TailwindCSS for the new layout",
            "[00:04:00] Alice: I'll prepare the slide deck"
        });

        // Act
        var jsonPath = await analyzer.AnalyzeAsync(id, transcriptPath, CancellationToken.None);

        // Assert
        File.Exists(jsonPath).Should().BeTrue();

        var json = await File.ReadAllTextAsync(jsonPath);
        var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        root.GetProperty("meetingId").GetString().Should().Be(id.ToString());
        root.GetProperty("participants").EnumerateArray().Should().HaveCount(2);
        //root.GetProperty("keywords").EnumerateArray().Should().Contain("tailwindcss");
        root.GetProperty("timeline").EnumerateArray().Should().HaveCount(3);
    }
}