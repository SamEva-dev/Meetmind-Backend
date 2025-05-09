using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Domain.Models;
using Meetmind.Domain.Models.Realtime;

namespace Meetmind.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyUpcomingAsync(UpcomingMeeting meeting, int minutesBefore, CancellationToken ct);
    Task RequestUserConfirmationAsync(Guid meetingId, string action, CancellationToken ct);
    Task NotifyActionResultAsync(ActionResultMessage message, CancellationToken ct);
}