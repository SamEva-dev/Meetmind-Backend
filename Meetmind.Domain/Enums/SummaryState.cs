using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Domain.Enums;

public enum SummaryState
{
    NotRequested,
    Queued,
    Processing,
    Completed,
    Failed
}
