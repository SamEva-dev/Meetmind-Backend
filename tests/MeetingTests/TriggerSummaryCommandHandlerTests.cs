using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Application.Commands;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests;

public class TriggerSummaryCommandHandlerTests
{
    private readonly IMeetingRepository _repo = Substitute.For<IMeetingRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<TriggerSummaryCommandHandler> _logger = Substitute.For<ILogger<TriggerSummaryCommandHandler>>();

    [Fact]
    public async Task Should_Queue_Summary()
    {
        var meeting = new Meeting("Résumé à faire", DateTime.UtcNow);
        _repo.GetByIdAsync(meeting.Id, Arg.Any<CancellationToken>()).Returns(meeting);

        var handler = new TriggerSummaryCommandHandler(_repo, _uow, _logger);
        await handler.Handle(new TriggerSummaryCommand(meeting.Id), default);

        meeting.SummaryState.Should().Be(SummaryState.Queued);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_If_Meeting_Not_Found()
    {
        var handler = new TriggerSummaryCommandHandler(_repo, _uow, _logger);

        var act = () => handler.Handle(new TriggerSummaryCommand(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}