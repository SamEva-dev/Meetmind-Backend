using MediatR;

namespace Meetmind.Application.Queries;

public record GetTranscriptQuery(Guid MeetingId) : IRequest<string>;