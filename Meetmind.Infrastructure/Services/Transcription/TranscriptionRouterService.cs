using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Services.Transcription;

public class TranscriptionRouterService : ITranscriptionService
{
    private readonly IReadOnlyList<ITranscriptionService> _services;
    private readonly MeetMindDbContext _db;

    public TranscriptionRouterService(GrpcTranscriptionService grpc,
        ProcessTranscriptionService process,
        InteropTranscriptionService interpo,
        MeetMindDbContext db)
    {
        _services = new List<ITranscriptionService> { process, interpo };
        _db = db;
    }

    public TranscriptionType BackendType => throw new NotSupportedException("Le routeur ne doit pas être sélectionné explicitement.");

    public async Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken cancellationToken)
    {
        var settings = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var type = settings?.TranscriptionType ?? TranscriptionType.Grpc;

        var service = _services.FirstOrDefault(s =>
            s.BackendType == type);

        if (service == null)
            throw new InvalidOperationException($"Aucun service trouvé pour le backend {type}");

        return await service.TranscribeAsync(meeting, cancellationToken);
    }
}
