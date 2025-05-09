using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Models.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Meetmind.Presentation.Hubs;

//[Authorize]
public class NotifyHub : Hub
{
    private readonly ILogger<NotifyHub> _logger;
    private readonly IMeetingOrchestratorManager _orchestratorManager;

    public NotifyHub(ILogger<NotifyHub> logger, IMeetingOrchestratorManager orchestratorManager)
    {
        _logger = logger;
        _orchestratorManager = orchestratorManager;
    }

    public async Task ConfirmAction(ConfirmActionMessage message)
    {
        _logger.LogInformation("SignalR: ConfirmAction received for Meeting {Id}, Action: {Action}, Accepted: {Accepted}",
            message.MeetingId, message.Action, message.Accepted);

        await _orchestratorManager.HandleConfirmationAsync(message, CancellationToken.None);
    }
}