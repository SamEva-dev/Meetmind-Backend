using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Dtos;
using Meetmind.Domain.Entities;

namespace Meetmind.Application.Common.Interfaces;

public interface IMeetingRepository
{
    Task<Meeting?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<MeetingDto>> GetMeetingsTodayAsync(DateTime today, CancellationToken ct);
}