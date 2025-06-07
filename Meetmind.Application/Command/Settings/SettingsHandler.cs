using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Events.Interface;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Settings
{
    public class SettingsHandler: IRequestHandler<SettingsCommand, SettingsDto>
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ILogger<SettingsHandler> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;
        private readonly IUnitOfWork _uow;

        public SettingsHandler(ISettingsRepository settingsRepository, ILogger<SettingsHandler> logger, IMapper mapper, INotificationService notification, IUnitOfWork uow)
        {
            _settingsRepository = settingsRepository;
            _logger = logger;
            _mapper = mapper;
            _notification = notification;
            _uow = uow;
        }

        public async Task<SettingsDto> Handle(SettingsCommand request, CancellationToken cancellationToken)
        {
           var settingEntity = _mapper.Map<SettingsEntity>(request);
            _logger.LogInformation("Creating settings with id: {Id}", settingEntity.Id);

            // Create new settings
            await _settingsRepository.DeleteAsync(cancellationToken);
            await _settingsRepository.SaveAsync(settingEntity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _notification.NotifySettingsUpdatedAsync(settingEntity, cancellationToken);

            await _notification.NotifyMeetingAsync(new Domain.Models.Notifications
            {
                MeetingId = settingEntity.Id,
                Title = "Settings Updated",
                Message = $"Settings have been updated successfully.",
                Time = DateTime.UtcNow
            }, cancellationToken);

            return _mapper.Map<SettingsDto>(settingEntity);
        }
    }
}
