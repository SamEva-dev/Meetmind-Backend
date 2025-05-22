
using Meetmind.Application.Services;
using Meetmind.Domain.Enums;
using Meetmind.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Services.Transcription;

public class GrpcTranscriptionService : ITranscriptionService
{
    //public TranscriptionType BackendType => TranscriptionType.Grpc;

    //private readonly ILogger<GrpcTranscriptionService> _logger;
    //private readonly IHubContext<TranscriptHub> _hubContext;
    //private readonly WhisperTranscription.WhisperTranscriptionClient _grpcClient;

    //public GrpcTranscriptionService(
    //    ILogger<GrpcTranscriptionService> logger,
    //    IHubContext<TranscriptHub> hubContext,
    //    WhisperTranscription.WhisperTranscriptionClient grpcClient)
    //{
    //    _logger = logger;
    //    _hubContext = hubContext;
    //    _grpcClient = grpcClient;
    //}

    //public async Task<TranscriptionResult> TranscribeAsync(string audioPath, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        _logger.LogInformation("gRPC transcription démarrée pour {Path}", audioPath);

    //        await _hubContext.Clients.All.SendAsync("TranscriptionProcessing", audioPath);

    //        var request = new TranscriptionRequest { AudioFilePath = audioPath };
    //        var response = await _grpcClient.TranscribeAsync(request, cancellationToken: cancellationToken);

    //        if (!string.IsNullOrEmpty(response.ErrorMessage))
    //            throw new Exception(response.ErrorMessage);

    //        await _hubContext.Clients.All.SendAsync("TranscriptionCompleted", new { audioPath, transcriptPath = response.TranscriptFilePath });
    //        _logger.LogInformation("gRPC transcription terminée : {TranscriptPath}", response.TranscriptFilePath);

    //        return new TranscriptionResult
    //        {
    //            OutputPath = response.TranscriptFilePath,
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "gRPC transcription FAILED pour {Path}", audioPath);
    //        await _hubContext.Clients.All.SendAsync("TranscriptionFailed", new { audioPath, error = ex.Message });
    //        throw;
    //    }
    //}
    public TranscriptionType BackendType => throw new NotImplementedException();


    public async Task<TranscriptionDto> TranscribeAsync(string audioPath, CancellationToken ct)
    {
        return new TranscriptionDto
        {
            OutputPath = "r",
        };
    }

    public Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
