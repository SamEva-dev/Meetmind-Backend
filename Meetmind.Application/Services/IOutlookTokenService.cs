
using Azure.Core;
using Meetmind.Domain.Models;

namespace Meetmind.Application.Services;

public interface IOutlookTokenService
{
    TokenCredential CreateCachedCredential(OutlookCredentials config);
}
