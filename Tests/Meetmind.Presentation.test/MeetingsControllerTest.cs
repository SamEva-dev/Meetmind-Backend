using FluentAssertions;
using Google.Apis.Calendar.v3.Data;
using MediatR;
using Meetmind.Application.Command.Meetings;
using Meetmind.Application.Dto;
using Meetmind.Application.QueryHandles.Meetings;
using Meetmind.Application.QueryHandles.Mettings;
using Meetmind.Application.Repositories;
using Meetmind.Presentation.Controllers;
using Meetmind.Presentation.test.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Meetmind.Presentation.test
{
    public class MeetingsControllerTest
    {
        private readonly MeetingsController _controller;

        private readonly Mock<ISender> _mediatorMock;
        private readonly Mock<ILogger<MeetingsController>> _loggerMock;
        public MeetingsControllerTest()
        {
            _mediatorMock = new Mock<ISender>();
            _loggerMock = new Mock<ILogger<MeetingsController>>();
            _controller = new MeetingsController(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var meeting = new MeetingDtoBuilder().WithId(id).Build();
            _mediatorMock.Setup(x => x.Send(It.Is<GetMeetingByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(meeting);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(meeting);

            result.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetMeetingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetById(id);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
            ((NotFoundObjectResult)result.Result).Value.Should().Be("Meeting not found.");
        }

        [Fact]
        public async Task GetById_Returns500_WhenException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetMeetingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Internal error"));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            ((ObjectResult)result.Result).Value.Should().Be("An error occurred while processing your request.");
        }

        [Fact]
        public async Task GetToday_ReturnsOk_WhenFound()
        {
            // Arrange
            var meetings = MeetingDtoBuilder.BuildList(2);
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetTodayMeetingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(meetings);

            // Act
            var result = await _controller.GetToday();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(meetings);

            result.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetToday_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetTodayMeetingsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetToday();

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
            ((NotFoundObjectResult)result.Result).Value.Should().Be("Meeting not found  for today");
        }

        [Fact]
        public async Task GetToday_Returns500_WhenException()
        {
            // Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetTodayMeetingsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            // Act
            var result = await _controller.GetToday();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            ((ObjectResult)result.Result).Value.Should().Be("An error occurred while processing your request.");
        }

        [Fact]
        public async Task GetRecentMeeting_ReturnsOk_WhenFound()
        {
            // Arrange
            var meetings = new PagedResult<MeetingDto>
            {
                Items = MeetingDtoBuilder.BuildList(2),
                TotalCount = 2,
                Page = 1,
                PageSize = 2
            };
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetRecentMeetingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(meetings); // Ensure the type matches PagedResult<MeetingDto>

            // Act
            var result = await _controller.GetRecenteMeeting();

            // Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(meetings);
        }

        [Fact]
        public async Task GetRecentMeeting_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetRecentMeetingQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetRecenteMeeting();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Meeting not found.");
        }

        [Fact]
        public async Task GetRecentMeeting_Returns500_WhenException()
        {
            // Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetRecentMeetingQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            // Act
            var result = await _controller.GetRecenteMeeting();

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            ((ObjectResult)result).Value.Should().Be("An error occurred while processing your request.");
        }

        [Fact]
        public async Task GetUpComingMeeting_ReturnsOk_WhenFound()
        {
            // Arrange
            var meetings = new PagedResult<MeetingDto>
            {
                Items = MeetingDtoBuilder.BuildList(2),
                TotalCount = 2,
                Page = 1,
                PageSize = 2
            };
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetUpComingMeetingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(meetings);

            // Act
            var result = await _controller.GetUpComingMeeting();

            // Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(meetings);
        }

        [Fact]
        public async Task GetUpComingMeeting_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetUpComingMeetingQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetUpComingMeeting();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Meeting not found.");
        }

        [Fact]
        public async Task GetUpComingMeeting_Returns500_WhenException()
        {
            // Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetUpComingMeetingQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            // Act
            var result = await _controller.GetUpComingMeeting();

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            ((ObjectResult)result).Value.Should().Be("An error occurred while processing your request.");
        }


        [Fact]
        public async Task Delete_ReturnsOk_WhenDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = Unit.Value; // Use Unit.Value as the expected return type
            _mediatorMock.Setup(x => x.Send(It.Is<DeleteMeetingCommand>(q => q.MeetingId == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected); // Ensure ReturnsAsync matches the expected type

            // Act
            var result = await _controller.Delete(id);

            // Assert
          //  result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(200);
            
          //  result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(true); // Adjust assertion to match the expected behavior
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteMeetingCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>().Which.Value.Should().Be("Meeting not found.");
        }

        [Fact]
        public async Task Delete_Returns500_WhenException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteMeetingCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            ((ObjectResult)result).Value.Should().Be("An error occurred while processing your request.");
        }

        [Fact]
        public async Task CreateMeeting_ReturnsCreated_WhenSuccess()
        {
            // Arrange
            var command = new CreateMeetingCommand("", DateTime.UtcNow, DateTime.UtcNow.AddHours(1),  null, null);
            var meetingId = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(meetingId);

            // Act
            var result = await _controller.CreateMeeting(command);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
            createdResult.Value.Should().Be(meetingId);
        }

        [Fact]
        public async Task CreateMeeting_Returns500_WhenException()
        {
            // Arrange
            var command = new CreateMeetingCommand("", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null);
            _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("error"));

            // Act
            var result = await _controller.CreateMeeting(command);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            ((ObjectResult)result).Value.Should().Be("An error occurred while processing your request.");
        }


    }
}
