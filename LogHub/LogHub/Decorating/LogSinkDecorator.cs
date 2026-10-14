using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Decorating;
public abstract class LogSinkDecorator : ILogSink, IFlushableLogSink
{
    protected ILogSink Inner { get; }
    private bool _disposed;
    public string Name => Inner.Name;
    protected LogSinkDecorator(ILogSink inner) => Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    protected void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
    public virtual void Write(LogRecord record, string text) { CheckDisposed(); Inner.Write(record, text); }
    public virtual void Flush()
    {
        CheckDisposed();
        if (Inner is IFlushableLogSink flushable) flushable.Flush();
    }
    public void Dispose()
    {
        if (_disposed) return;
        try { Flush(); }
        finally { _disposed = true; Inner.Dispose(); }
    }
}
