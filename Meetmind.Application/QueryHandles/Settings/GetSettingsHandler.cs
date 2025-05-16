
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Settings;

public class GetSettingsHandler : IRequestHandler<GetSettingsQuery, SettingsDto>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<GetSettingsHandler> _logger;
    public GetSettingsHandler(ISettingsRepository settingsRepository,
        ILogger<GetSettingsHandler> logger)
    {
        _settingsRepository = settingsRepository;
        _logger = logger;
    }
    public async Task<SettingsDto> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetSettingsQuery");
        try
        {
            var settings = await _settingsRepository.GetAllAsync(cancellationToken);

            if (settings == null)
            {
                _logger.LogWarning("Settings not found ");
                throw new KeyNotFoundException($"Settings  not found.");
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling GetSettingsQuery ");
            throw;
        }
        finally
        {
            _logger.LogInformation("Finished handling GetSettingsQuery ");
        }
    }
}