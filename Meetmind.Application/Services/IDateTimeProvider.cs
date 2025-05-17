using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Application.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Today => UtcNow.Date;
}
