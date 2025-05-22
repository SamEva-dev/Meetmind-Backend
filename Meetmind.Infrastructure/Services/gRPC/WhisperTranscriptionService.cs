using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services.gRPC;

// Corrected: Ensure the base class is properly referenced
public class WhisperTranscriptionService //: WhisperTranscription.WhisperTranscriptionBase
{
    //private readonly ILogger<WhisperTranscriptionService> _logger;

    //public WhisperTranscriptionService(ILogger<WhisperTranscriptionService> logger)
    //{
    //    _logger = logger;
    //}

    //public override async Task<TranscriptionResponse> Transcribe(
    //    TranscriptionRequest request,
    //    ServerCallContext context)
    //{
    //    string? transcriptFilePath = null;
    //    string? errorMessage = null;

    //    try
    //    {
    //        transcriptFilePath = Path.ChangeExtension(request.AudioFilePath, ".txt");
    //        await File.WriteAllTextAsync(transcriptFilePath, $"Transcription fake de : {request.AudioFilePath}");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Erreur lors de la transcription");
    //        errorMessage = ex.Message;
    //    }

    //    return new TranscriptionResponse
    //    {
    //        TranscriptFilePath = transcriptFilePath ?? "",
    //        ErrorMessage = errorMessage ?? ""
    //    };
    //}
}
