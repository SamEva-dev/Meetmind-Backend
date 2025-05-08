using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Meetmind.Application.Queries;

public record GetSummaryQuery(Guid MeetingId) : IRequest<string>;