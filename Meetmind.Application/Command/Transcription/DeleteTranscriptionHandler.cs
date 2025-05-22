using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Transcription
{
    public class DeleteTranscriptionHandler : IRequestHandler<DeleteTranscriptionCommand, Unit>
    {
        private readonly ITranscriptionRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTranscriptionHandler> _logger;

        public DeleteTranscriptionHandler(ITranscriptionRepository repo, IUnitOfWork unitOfWork, ILogger<DeleteTranscriptionHandler> logger)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteTranscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting transcription {TranscriptionId}", request.MeetingId);
            var transcription = await _repo.GetTranscriptionByIdAsync(request.MeetingId, cancellationToken);
            if (transcription == null)
            {
                _logger.LogInformation("Transcription {TranscriptionId} not found", request.MeetingId);
                throw new KeyNotFoundException($"Transcription {request.MeetingId} not found");
            }
            await _repo.DeleteTransition(transcription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

}
