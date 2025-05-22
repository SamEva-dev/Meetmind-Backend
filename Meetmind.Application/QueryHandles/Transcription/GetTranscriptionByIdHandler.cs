using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Transcription;

public class GetTranscriptionByIdHandler : IRequestHandler<GetTranscriptionByIdQuery, TranscriptionDto>
{
    private readonly ITranscriptionRepository _repository;
    private readonly ILogger<GetTranscriptionByIdHandler> _logger;

    public GetTranscriptionByIdHandler(ITranscriptionRepository repository, ILogger<GetTranscriptionByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TranscriptionDto> Handle(GetTranscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération des transcriptions");
        var transcription = await _repository.GetTranscriptionByIdAsync(request.meetingId, cancellationToken);
        if (transcription == null)
        {
            _logger.LogWarning("Aucune transcription trouvée");
            throw new KeyNotFoundException("Aucune transcription trouvée");
        }
        return transcription;
    }
}
