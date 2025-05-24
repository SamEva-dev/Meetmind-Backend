
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Meetmind.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services.Summarize;

public class AudioSummarizeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AudioSummarizeService> _logger;
    private readonly MeetMindDbContext _db;

    public AudioSummarizeService(HttpClient httpClient, ILogger<AudioSummarizeService> logger, MeetMindDbContext db)
    {
        _httpClient = httpClient;
        _logger = logger;
        _db = db;
    }

    public async Task<string> SummarizeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        try
        {
            var transcription = await _db.Transcriptions
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(t => t.MeetingId == meeting.Id);
            if (transcription == null)
            {
                _logger.LogWarning("Aucune transcription trouvée pour la réunion {Id}", meeting.Id);
                throw new Exception("Aucune transcription trouvée pour la réunion " + meeting.Id);
            }

            var setting = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);

            var requestBody = new
            {
                text = transcription.Text,
                language = setting?.Language,
                summary_model = EnumHelper.GetEnumValueForPython(setting.SummarizeModelType)
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            // Remplace par l’URL de ton worker !
            var response = await _httpClient.PostAsync("summarize", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Erreur lors de l'appel au résumé : {StatusCode} - {Message}", response.StatusCode, err);
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("summary", out var summaryProp))
            {
                var summary = summaryProp.GetString();
                _logger.LogInformation("Résumé reçu, taille {Length} caractères", summary?.Length ?? 0);
                return summary;
            }
            else
            {
                _logger.LogWarning("La réponse de l’API de résumé ne contient pas de champ 'summary'");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans SummarizeTranscriptionAsync");
            return null;
        }
    }
}
