using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Application.Common.Interfaces;

public interface ISummaryService
{
    Task<string> GenerateSummaryAsync(Guid meetingId, CancellationToken ct);
}