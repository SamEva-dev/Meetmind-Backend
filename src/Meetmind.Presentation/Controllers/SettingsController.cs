using AutoMapper;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetmind.Presentation.Controllers
{
    [Route("v1/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IUserSettingsRepository _repo;
        private readonly IMapper _mapper;

        public SettingsController(IUserSettingsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // TODO: à remplacer par ICurrentUserService.UserId
        private static readonly Guid USER_ID = Guid.Parse("00000000-0000-0000-0000-000000000001");

        [HttpGet]
        public async Task<ActionResult<UserSettingsDto>> GetSettings(CancellationToken ct)
        {
            var entity = await _repo.GetAsync(USER_ID, ct);
            return Ok(_mapper.Map<UserSettingsDto>(entity));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UserSettingsDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetAsync(USER_ID, ct);
            _mapper.Map(dto, entity);
            await _repo.SaveAsync(entity, ct); // à ajouter si pas encore dans le repo
            return NoContent();
        }
    }
}
