using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Settings
{
    public class SettingsHandler: IRequestHandler<SettingsCommand, SettingsDto>
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ILogger<SettingsHandler> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;
        public SettingsHandler(ISettingsRepository settingsRepository,
            INotificationService notification,
            IMapper mapper, ILogger<SettingsHandler> logger)
        {
            _settingsRepository = settingsRepository;
            _mapper = mapper;
            _logger = logger;
            _notification = notification;
        }
       
        public async Task<SettingsDto> Handle(SettingsCommand request, CancellationToken cancellationToken)
        {
           var settingEntity = _mapper.Map<SettingsEntity>(request);
            _logger.LogInformation("Creating settings with id: {Id}", settingEntity.Id);

            // Create new settings
            await _settingsRepository.DeleteAsync(cancellationToken);
            await _settingsRepository.SaveAsync(settingEntity, cancellationToken);
            await _settingsRepository.ApplyAsync(cancellationToken);

            var dto = _mapper.Map<SettingsDto>(settingEntity);
            await _notification.NotifySettingsUpdatedAsync(dto);

            return dto;
        }
    }
}
