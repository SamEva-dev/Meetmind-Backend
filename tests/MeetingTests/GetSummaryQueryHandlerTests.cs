using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Queries;
using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests;

public class GetSummaryQueryHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly ILogger<GetSummaryQueryHandler> _logger = Substitute.For<ILogger<GetSummaryQueryHandler>>();

    [Fact]
    public async Task Should_Read_Summary_From_File()
    {
        var id = Guid.NewGuid();
        var path = $"Data/Summary/{id}.md";
        Directory.CreateDirectory("Data/Summary");
        await File.WriteAllTextAsync(path, "# Résumé complet");

        var meeting = new Meeting("Demo", DateTime.UtcNow);
        meeting.QueueSummary();
        meeting.MarkSummaryProcessing();
        meeting.MarkSummaryCompleted(path);

        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new GetSummaryQueryHandler(_repo, _logger);
        var result = await handler.Handle(new GetSummaryQuery(id), default);

        result.Should().Contain("Résumé");
    }

    [Fact]
    public async Task Should_Throw_If_Not_Ready()
    {
        var id = Guid.NewGuid();
        var meeting = new Meeting("No summary yet", DateTime.UtcNow);

        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new GetSummaryQueryHandler(_repo, _logger);
        var act = () => handler.Handle(new GetSummaryQuery(id), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}