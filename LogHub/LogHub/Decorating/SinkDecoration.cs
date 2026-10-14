using LogHub.Configuration;
using LogHub.Sinks;
namespace LogHub.Decorating;
public static class SinkDecoration
{
    public static ILogSink Apply(ILogSink sink, DeliveryOptions options, TimeProvider clock,
        Action<WriteMeasurement> report)
    {
        var names = options.Decorators.Select(x => x.Trim().ToLowerInvariant()).ToArray();
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
            throw new ArgumentException("A decorator cannot be listed twice.");
        if (names.Any(x => x is not ("buffer" or "retry" or "timing")))
            throw new ArgumentException("Supported decorators: buffer, retry, timing.");
        if (options.BatchSize < 1 || options.Retry.Attempts < 1)
            throw new ArgumentException("Batch size and retry attempts must be positive.");
        foreach (var name in names.Reverse())
            sink = name switch
            {
                "buffer" => new BufferedLogSink(sink, options.BatchSize),
                "retry" => new RetryLogSink(sink, options.Retry.Attempts),
                "timing" => new TimingLogSink(sink, clock, report),
                _ => throw new InvalidOperationException("Decorator validation failed.")
            };
        return sink;
    }
}
