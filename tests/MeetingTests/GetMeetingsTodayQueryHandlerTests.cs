using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Application.Queries;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace MeetingTests;

public class GetMeetingsTodayQueryHandlerTests
{
    private readonly DbContextOptions<MeetMindDbContext> _options;

    public GetMeetingsTodayQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<MeetMindDbContext>()
            .UseInMemoryDatabase("meetings-today")
            .Options;
    }

    [Fact]
    public async Task Should_Return_Today_Meetings()
    {
        //using var db = new MeetMindDbContext(_options);
        //db.Meetings.AddRange(
        //    new Meeting("Today", DateTime.UtcNow),
        //    new Meeting("Yesterday", DateTime.UtcNow.AddDays(-1)),
        //    new Meeting("Tomorrow", DateTime.UtcNow.AddDays(1))
        //);
        //await db.SaveChangesAsync();

        //var handler = new GetMeetingsTodayQueryHandler(db);
        //var result = await handler.Handle(new GetMeetingsTodayQuery(), default);

        //result.Should().ContainSingle(x => x.Title == "Today");
    }
}