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
    private readonly MeetMindDbContext _db;
    private const string ACCESS_TOKEN = "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";
    private readonly HttpClient _httpClient;
    private readonly AudioTranscriptionService _audioTranscriptionService;

    public ProcessTranscriptionService(AudioTranscriptionService audioTranscriptionService, ILogger<ProcessTranscriptionService> logger, MeetMindDbContext db)
    {
        _logger = logger;
        _db = db;
        _audioTranscriptionService = audioTranscriptionService;
    }

    public async Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        //var result = await ProcessOneSHot(meeting, ct);
        var result = await _audioTranscriptionService.TranscribeAsync(meeting, ct);
        return result;
    }


    private async Task<TranscriptionDto> ProcessOneSHot(MeetingEntity meeting, CancellationToken ct)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "transcribe.py");

        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script non trouvé: {scriptPath}");

        var path = "E:\\Meetmind\\Meetmind-Backend\\Meetmind\\bin\\Debug\\net9.0\\Resources\\audio\\test.wav";
        // Commande à exécuter
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\" \"{path}\" --device cpu --compute_type int8 --hf_token {ACCESS_TOKEN} --diarization_model pyannote/speaker-diarization@2.1", // (adapter selon ton script)
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = new Process { StartInfo = psi };
        proc.Start();

        string output = await proc.StandardOutput.ReadToEndAsync();
        string error = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync(ct);

        if (proc.ExitCode != 0)
        {
            _logger.LogError("Erreur process transcription: {Error}", error);
            throw new Exception($"Transcription process failed: {error}");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<TranscriptionDto>(output, options);

        if (result == null || !result.Success)
            throw new Exception(result?.Error ?? "Transcription failed");


        var transcription = new TranscriptionEntity
        {
            MeetingId = meeting.Id,
            Tilte = meeting.Title,
            SourceFile = meeting.AudioPath,
            Text = result.Text,
            Language = result.Language,
            LanguageProbability = result.Language_probability,
            Duration = result.Duration,
            CreatedAt = DateTime.UtcNow,
            Segments = result.Segments?.Select(s => new TranscriptionSegment
            {
                TranscriptionId = meeting.Id,
                Start = s.Start,
                End = s.End,
                Text = s.Text,
                Speaker = s.Speaker,
            }).ToList()
        };

        await _db.Transcriptions.AddAsync(transcription, ct);

        await _db.SaveChangesAsync(ct);

        return result;
    }
}
