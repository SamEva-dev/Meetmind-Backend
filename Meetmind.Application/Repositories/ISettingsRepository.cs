
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Application.Repositories
{
    public interface ISettingsRepository: IDisposable
    {
        Task SaveAsync(SettingsEntity existingSettings, CancellationToken cancellationToken);
        Task<SettingsDto> GetAllAsync(CancellationToken cancellationToken);

        Task ApplyAsync(CancellationToken cancellationToken);
        Task DeleteAsync(CancellationToken cancellationToken);
    }
}
