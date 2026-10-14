using LogHub.Decorating;
using LogHub.Observing;
using LogHub.Sinks;
namespace LogHub.Processing;
public sealed class PipelineSinkLifetime : IDisposable
{
    private readonly ILogSink[] _sinks;
    private readonly PipelineEvents _events;
    private bool _disposed;
    public List<WriteMeasurement> Measurements { get; }
    public PipelineSinkLifetime(IEnumerable<ILogSink> sinks, PipelineEvents events, List<WriteMeasurement> measurements)
        => (_sinks, _events, Measurements) = (sinks.ToArray(), events, measurements);
    public void Flush()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (var sink in _sinks)
        {
            try { if (sink is IFlushableLogSink flushable) flushable.Flush(); }
            catch (Exception error) { _events.ReportFailure(sink.Name + ".Flush", error); }
        }
    }
    public int ErrorCount => _events.Failures.Count;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        var errors = new List<Exception>();
        foreach (var sink in _sinks.Reverse())
        {
            try { sink.Dispose(); }
            catch (Exception error) { errors.Add(error); }
        }
        if (errors.Count != 0) throw new AggregateException("Receiver disposal failed; delivery is not confirmed.", errors);
    }
}
