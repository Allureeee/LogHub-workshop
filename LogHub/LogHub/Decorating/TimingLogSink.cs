using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Decorating;
public sealed record WriteMeasurement(string Sink, TimeSpan Elapsed, bool Succeeded);
public sealed class TimingLogSink : LogSinkDecorator
{
    private readonly TimeProvider _clock;
    private readonly Action<WriteMeasurement> _report;
    public TimingLogSink(ILogSink inner, TimeProvider clock, Action<WriteMeasurement> report) : base(inner)
        => (_clock, _report) = (clock ?? throw new ArgumentNullException(nameof(clock)),
            report ?? throw new ArgumentNullException(nameof(report)));
    public override void Write(LogRecord record, string text)
    {
        CheckDisposed();
        var start = _clock.GetTimestamp();
        var succeeded = false;
        try { Inner.Write(record, text); succeeded = true; }
        finally { _report(new WriteMeasurement(Name, _clock.GetElapsedTime(start), succeeded)); }
    }
}
