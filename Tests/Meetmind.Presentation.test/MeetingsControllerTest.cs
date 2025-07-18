using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.QueryHandles.Mettings;
using Meetmind.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Meetmind.Presentation.test
{
    public class MeetingsControllerTest
    {
        private readonly MeetingsController _meetingsController;

        private readonly Mock<ISender> _mediatorMock;
        private readonly Mock<ILogger<MeetingsController>> _loggerMock;
        public MeetingsControllerTest()
        {
            _mediatorMock = new Mock<ISender>();
            _loggerMock = new Mock<ILogger<MeetingsController>>();
            _meetingsController = new MeetingsController(_mediatorMock.Object, _loggerMock.Object);
        }
        [Fact]
        public void  GetById_ReturnOk_WhenIdExist()
        {
            // Arrange
            
            var item = new MeetingDto
            {
                Id = Guid.NewGuid(),
                Title = "Test Meeting",
                StartUtc = DateTime.UtcNow,
                EndUtc = DateTime.UtcNow.AddHours(1),
                State = "Scheduled",
                TranscriptState = "NotAvailable",
                SummaryState = "NotAvailable",
                AudioPath = null,
                TranscriptPath = null,
                SummaryPath = null
            };
            var result = _mediatorMock.Setup(x => x.Send(It.IsAny<GetMeetingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);
            var id = Guid.NewGuid();
            // Act
            var actionResult = _meetingsController.GetById(id).Result;

            // Assert
            Assert.NotNull(actionResult);
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(item);
        }

        [Fact]
        public void GetById_ReturnNotFound_WhenIdExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetMeetingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());
            // Act
            var actionResult = _meetingsController.GetById(id).Result;
            // Assert
            Assert.NotNull(actionResult);
            var notFoundResult = actionResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("Meeting not found.");
        }
    }
}
