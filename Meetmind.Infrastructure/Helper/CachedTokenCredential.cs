using System.Text.Json;
using Azure.Core;

namespace Meetmind.Infrastructure.Helper;

public class CachedTokenCredential : TokenCredential
{
    private readonly TokenCredential _inner;
    private readonly string[] _scopes;
    private readonly string _cachePath;

    public CachedTokenCredential(TokenCredential inner, string[] scopes, string cachePath)
    {
        _inner = inner;
        _scopes = scopes;
        _cachePath = cachePath;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        if (File.Exists(_cachePath))
        {
            var json = await File.ReadAllTextAsync(_cachePath, cancellationToken);
            var cached = JsonSerializer.Deserialize<CachedToken>(json);
            if (cached != null && cached.ExpiresOn > DateTimeOffset.UtcNow)
            {
                return new AccessToken(cached.Token, cached.ExpiresOn);
            }
        }

        var token = await _inner.GetTokenAsync(requestContext, cancellationToken);
        var data = new CachedToken { Token = token.Token, ExpiresOn = token.ExpiresOn };

        await File.WriteAllTextAsync(_cachePath, JsonSerializer.Serialize(data), cancellationToken);
        return token;
    }

    private class CachedToken
    {
        public string Token { get; set; } = default!;
        public DateTimeOffset ExpiresOn { get; set; }
    }
}
