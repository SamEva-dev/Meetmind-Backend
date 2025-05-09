using AutoMapper;
using FluentAssertions;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Dtos;
using Meetmind.Domain.Entities;
using Meetmind.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MeetingTests.Integration.Api.Controllers;

public class SettingsControllerTests
{
    private readonly IUserSettingsRepository _repo = Substitute.For<IUserSettingsRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    [Fact]
    public async Task GetSettings_Should_Return_Dto()
    {
        var entity = new UserSettingsEntity { AutoStartRecord = true };
        var dto = new UserSettingsDto { AutoStartRecord = true };

        _repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entity);
        _mapper.Map<UserSettingsDto>(entity).Returns(dto);

        var ctrl = new SettingsController(_repo, _mapper);
        var result = await ctrl.GetSettings(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result.Result!).Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task UpdateSettings_Should_Save_Changes()
    {
        var entity = new UserSettingsEntity();
        var dto = new UserSettingsDto { AutoTranscript = true };

        _repo.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(entity);
        _mapper.When(m => m.Map(dto, entity)).Do(_ => entity.AutoTranscript = true);

        var ctrl = new SettingsController(_repo, _mapper);
        var result = await ctrl.UpdateSettings(dto, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await _repo.Received().SaveAsync(entity, Arg.Any<CancellationToken>());
        entity.AutoTranscript.Should().BeTrue();
    }
}