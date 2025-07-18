using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Http;
using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Enums;

namespace Meetmind.Application.Command.Admin
{
    public class DeleteMeetingHandler : IRequestHandler<DeleteMeetingCommand, Unit>
    {
        private readonly IMeetingRepository _repo;
        public DeleteMeetingHandler(IMeetingRepository repo)
        {
            _repo = repo;
        }

        public async Task<Unit> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _repo.ExecuteSqlRawAsync(ExecuteType.DELETE);

            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression de la table : {ex.Message}");
            }

            return Unit.Value;
        }
    }
}
