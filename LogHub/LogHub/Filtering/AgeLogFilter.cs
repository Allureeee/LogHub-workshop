using LogHub.Configuration;
using LogHub.Models;
namespace LogHub.Filtering;

public sealed class AgeLogFilter : LogFilter
{
    private readonly TimeProvider _clock;
    private readonly TimeSpan _maxAge;
    public AgeLogFilter(LogConfiguration configuration, TimeProvider clock)
    {
        if (configuration.MaxAgeMinutes < 0) throw new ArgumentOutOfRangeException(nameof(configuration));
        _clock = clock;
        _maxAge = TimeSpan.FromMinutes(configuration.MaxAgeMinutes);
    }
    public override string Name => "age";
    protected override bool Allows(LogRecord entry)
    {
        var utc = entry.Date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc) : entry.Date.ToUniversalTime();
        return _clock.GetUtcNow() - new DateTimeOffset(utc) <= _maxAge;
    }
}
