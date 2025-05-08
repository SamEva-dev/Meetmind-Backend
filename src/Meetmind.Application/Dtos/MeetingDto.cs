using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Domain.Enums;

namespace Meetmind.Application.Dtos;

public class MeetingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public MeetingState State { get; set; }
}