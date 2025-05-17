
namespace Meetmind.Application.Workers;

public interface ICalendarWorker
{
    Task StartAsync(CancellationToken cancellationToken);
}
