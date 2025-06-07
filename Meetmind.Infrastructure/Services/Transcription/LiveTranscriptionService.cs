
using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Meetmind.Infrastructure.Services.Transcription;

public class LiveTranscriptionService : ILiveTranscriptionService
{
    private readonly MeetMindDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public LiveTranscriptionService(MeetMindDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    public async Task TranscribeAndStoreAsync(Guid meetingId, string fragmentPath, CancellationToken ct)
    {
        // Appel au service Python
        using var client = _httpClientFactory.CreateClient("TranscriptionAPI");
        using var form = new MultipartFormDataContent();
        using var audioStream = File.OpenRead(fragmentPath);

        form.Add(new StreamContent(audioStream), "audio", Path.GetFileName(fragmentPath));
        // Ajoute d'autres paramètres si besoin (langue, modèle...)

        var response = await client.PostAsync("transcribe", form, ct);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Erreur transcription live : " + content);

        var result = JsonSerializer.Deserialize<TranscriptionDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Cherche une transcription existante pour ce meeting, ou crée-la
        var transcription = await _db.Transcriptions
            .Include(t => t.Segments)
            .FirstOrDefaultAsync(t => t.MeetingId == meetingId, ct);

        if (transcription == null)
        {
            transcription = new TranscriptionEntity
            {
                MeetingId = meetingId,
                Tilte = "", // Renseigne selon ta logique
                SourceFile = fragmentPath,
                Text = result.Text,
                Language = result.Language,
                LanguageProbability = result.Language_probability,
                Duration = result.Duration,
                CreatedAt = DateTime.UtcNow,
                Speakers = result.Speakers ?? new List<string>(),
                Segments = new List<TranscriptionSegment>()
            };
            _db.Transcriptions.Add(transcription);
        }
        else
        {
            // On concatène le texte live
            transcription.Text += " " + result.Text;
            transcription.Speakers = result.Speakers ?? transcription.Speakers;
        }

        // Ajoute les segments
        if (result.Segments != null)
        {
            foreach (var s in result.Segments)
            {
                // Option : vérifie que ce segment n'existe pas déjà (évite les doublons si besoin)
                transcription.Segments.Add(new TranscriptionSegment
                {
                    TranscriptionId = transcription.Id,
                    Speaker = s.Speaker,
                    Text = s.Text,
                    Start = s.Start,
                    End = s.End
                });
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
