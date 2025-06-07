using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Mettings;

public sealed class GetTodayMeetingsHandler : IRequestHandler<GetTodayMeetingsQuery, List<MeetingDto>>
{
    private readonly IMeetingRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodayMeetingsHandler> _logger;
    private readonly INotificationService _recordingNotifierService;

    public GetTodayMeetingsHandler(IMeetingRepository repository,
        INotificationService recordingNotifierService,
        IMapper mapper, ILogger<GetTodayMeetingsHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _recordingNotifierService = recordingNotifierService;
    }

    public async Task<List<MeetingDto>> Handle(GetTodayMeetingsQuery request, CancellationToken cancellationToken)
    {
        var meetings = await _repository.GetMeetingToday(cancellationToken);
        if (meetings is null)
        {
            _logger.LogWarning("Not meeting today found");
            throw new KeyNotFoundException("Meeting not found");
        }

        await _recordingNotifierService.NotifyMeetingAsync(new Domain.Models.Notifications
        {
            MeetingId = Guid.NewGuid(),
            Title = "Tilte",
            Message = $"Meeting  has been getted successfully.",
            Time = DateTime.UtcNow
        }, cancellationToken);

        return meetings;
    }
}
