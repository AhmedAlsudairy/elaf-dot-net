namespace Munaqasat.Web.Services;

public interface IClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}

public sealed class SystemClock : IClock
{
    private static readonly TimeSpan OmanOffset = TimeSpan.FromHours(4);

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(OmanOffset).Date);
}
