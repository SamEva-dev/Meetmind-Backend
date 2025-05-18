namespace Meetmind.Application.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Today => UtcNow.Date;
}
