
using MediatR;

namespace Meetmind.Application.Command.Meetings;

public sealed record CreateMeetingCommand(
    string Title,
    DateTime Start,
    DateTime? End,
    string? ExternalId,
     string? ExternalSource) : IRequest<Guid>;

