using MediatR;
using Meetmind.Application.Dto;
using Microsoft.AspNetCore.Http;

namespace Meetmind.Application.Command.Audio;

public record AddAudioFragmentCommand(Guid MeetingId, int SequenceNumber, IFormFile AudioChunk) : IRequest<Unit>;
