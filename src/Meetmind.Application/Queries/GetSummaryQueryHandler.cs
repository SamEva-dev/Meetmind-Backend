using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Queries;

public class GetSummaryQueryHandler : IRequestHandler<GetSummaryQuery, string>
{
    private readonly IMeetingRepository _repo;
    private readonly ILogger<GetSummaryQueryHandler> _logger;

    public GetSummaryQueryHandler(IMeetingRepository repo, ILogger<GetSummaryQueryHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<string> Handle(GetSummaryQuery request, CancellationToken ct)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct)
            ?? throw new KeyNotFoundException();

        if (meeting.SummaryState != Domain.Enums.SummaryState.Completed || string.IsNullOrWhiteSpace(meeting.SummaryPath))
            throw new InvalidOperationException("Summary not available");

        if (!File.Exists(meeting.SummaryPath))
            throw new FileNotFoundException("Summary file missing");

        return await File.ReadAllTextAsync(meeting.SummaryPath, ct);
    }
}