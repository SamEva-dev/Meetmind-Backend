using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Meetmind.Application.Command.Admin
{
    public record DropMeetingCommand() : IRequest<Unit>
    {
    }
}
