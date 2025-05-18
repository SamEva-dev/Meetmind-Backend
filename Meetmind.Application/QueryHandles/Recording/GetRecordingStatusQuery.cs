using MediatR;

namespace Meetmind.Application.QueryHandles.Recording;

public sealed record GetRecordingStatusQuery(Guid MeetingId) : IRequest<string>;
