
namespace Meetmind.Domain.Models;

public sealed class OutlookCredentials
{
    public string ClientId { get; set; } = default!;
    public string TenantId { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string[] Scopes { get; set; } = default!;
}