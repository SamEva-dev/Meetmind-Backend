using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Dtos;

namespace Meetmind.Application.Queries;

public record GetMeetingsTodayQuery : IRequest<List<MeetingDto>>;