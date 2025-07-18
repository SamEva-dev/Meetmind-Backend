using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Enums;

namespace Meetmind.Application.Command.Admin
{
    public class DropMeetingHandler : IRequestHandler<DropMeetingCommand, Unit>
    {
        // This handler is not implemented yet.

        private readonly IMeetingRepository _repo;
        public DropMeetingHandler(IMeetingRepository repo)
        {
            _repo = repo;
        }
        public async Task<Unit> Handle(DropMeetingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _repo.ExecuteSqlRawAsync(ExecuteType.DROP);
               
            }
            catch (Exception ex)
            {
                throw new Exception( $"Erreur lors de la suppression de la table : {ex.Message}");
            }

            return Unit.Value;
        }
    }
}
