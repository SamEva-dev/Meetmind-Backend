using FluentAssertions;
using Meetmind.Infrastructure.Transcription;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests;

public class WhisperPythonServiceTests
{
    private readonly ILogger<WhisperPythonService> _logger = Substitute.For<ILogger<WhisperPythonService>>();

    [Fact]
    public async Task Should_Throw_If_Audio_File_Not_Exists()
    {
        var service = new WhisperPythonService(_logger);
        var id = Guid.NewGuid();

        var act = async () => await service.TranscribeAsync(id, CancellationToken.None);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task Should_Run_Python_And_Return_Transcript_Path()
    {
        // Arrange
        var id = Guid.NewGuid();
        var audioPath = Path.Combine("Data", "Audio", $"{id}.wav");
        var transcriptPath = Path.Combine("Data", "Transcript", $"{id}.txt");

        Directory.CreateDirectory("Data/Audio");
        Directory.CreateDirectory("Data/Transcript");

        // Simule un fichier audio
        await File.WriteAllBytesAsync(audioPath, new byte[] { 0x00 });

        // Crée un faux transcribe.py en PATH ou simulate output
        File.WriteAllText("transcribe.py", """
        import sys
        with open(sys.argv[2], 'w', encoding='utf-8') as f:
            f.write("Hello world")
        """);

        var service = new WhisperPythonService(_logger);

        // Act
        var resultPath = await service.TranscribeAsync(id, CancellationToken.None);

        // Assert
        resultPath.Should().Be(transcriptPath);
        File.Exists(resultPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(resultPath);
        content.Should().Contain("Hello world");

        // Cleanup
        File.Delete(audioPath);
        File.Delete(resultPath);
    }
}