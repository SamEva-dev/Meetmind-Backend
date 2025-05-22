using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Transcription
{
    public class GetTranscriptionHandler : IRequestHandler<GetTranscriptionQuery, List<TranscriptionDto>>
    {
        private readonly ITranscriptionRepository _repository;
        private readonly ILogger<GetTranscriptionHandler> _logger;

        public GetTranscriptionHandler(ITranscriptionRepository repository, ILogger<GetTranscriptionHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<TranscriptionDto>> Handle(GetTranscriptionQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Récupération des transcriptions");
            var transcriptions = await _repository.GetTranscriptionAsync(cancellationToken);
            if (transcriptions == null)
            {
                _logger.LogWarning("Aucune transcription trouvée");
                throw new KeyNotFoundException("Aucune transcription trouvée");
            }
            return transcriptions;
        }
    }
}
