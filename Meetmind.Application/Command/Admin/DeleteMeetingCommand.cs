using MediatR;

namespace Meetmind.Application.Command.Admin
{
    public class DeleteMeetingCommand() : IRequest<Unit>
    {
    }
}
