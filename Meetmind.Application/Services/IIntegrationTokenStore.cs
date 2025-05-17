
namespace Meetmind.Application.Services;

public interface IIntegrationTokenStore
{
    Task<string?> GetTokenAsync(string provider, string userEmail, CancellationToken ct);
}