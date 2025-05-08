using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;

namespace Meetmind.Application.Commands;

public record StartMeetingCommand(Guid MeetingId) : IRequest<Unit>;

