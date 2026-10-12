using LogHub.Sinks;
namespace LogHub.Observing;
public sealed class SinkObserver<TSink> : LogObserver where TSink : ILogSink
{
    private readonly TSink _sink;
    public SinkObserver(TSink sink) => _sink = sink;
    protected override void Observe(ProcessedLog item) => _sink.Write(item.Record, item.Text);
}
