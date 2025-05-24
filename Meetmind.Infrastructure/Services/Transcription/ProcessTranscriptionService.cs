using Meetmind.Application.Dto;
using System.Diagnostics;
using Meetmind.Application.Services;
using Meetmind.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Meetmind.Infrastructure.Repositories;
using Microsoft.Graph.Models.TermStore;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Meetmind.Infrastructure.Helper;

namespace Meetmind.Infrastructure.Services.Transcription;

public class ProcessTranscriptionService : ITranscriptionService
{
    public TranscriptionType BackendType => TranscriptionType.Process;
    private readonly ILogger<ProcessTranscriptionService> _logger;
    private readonly AudioTranscriptionService _audioTranscriptionService;

    public ProcessTranscriptionService(AudioTranscriptionService audioTranscriptionService, 
        ILogger<ProcessTranscriptionService> logger)
    {
        _logger = logger;
        _audioTranscriptionService = audioTranscriptionService;
    }

    public async Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        _logger.LogInformation("Début de la transcription pour la réunion {MeetingId}", meeting.Id);
        try
        {
            var result = await _audioTranscriptionService.TranscribeAsync(meeting, ct);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transcription for meeting {MeetingId}", meeting.Id);
            throw;
        }
    }
}
