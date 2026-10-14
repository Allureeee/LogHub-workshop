using LogHub.Models;
using LogHub.Sinks;
using LogHub.Decorating;
namespace LogHub.Proxy;
public sealed class VirtualLogSink : ILogSink, IFlushableLogSink
{
    private readonly Lazy<ILogSink> _real;
    private bool _disposed;
    public string Name { get; }
    public bool IsCreated => _real.IsValueCreated;
    public VirtualLogSink(string name, Func<ILogSink> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(factory);
        Name = name;
        _real = new Lazy<ILogSink>(() => factory() ?? throw new InvalidOperationException("The sink factory returned null."));
    }
    public void Write(LogRecord record, string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _real.Value.Write(record, text);
    }
    public void Flush()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_real.IsValueCreated && _real.Value is IFlushableLogSink flushable) flushable.Flush();
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_real.IsValueCreated) _real.Value.Dispose();
    }
}
