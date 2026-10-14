using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Decorating;
public sealed class RetryLogSink : LogSinkDecorator
{
    private readonly int _attempts;
    public RetryLogSink(ILogSink inner, int attempts) : base(inner)
    {
        if (attempts < 1) throw new ArgumentOutOfRangeException(nameof(attempts));
        _attempts = attempts;
    }
    public override void Write(LogRecord record, string text)
    {
        CheckDisposed();
        var resumeFlush = false;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                if (resumeFlush && Inner is IFlushableLogSink buffered) buffered.Flush();
                else Inner.Write(record, text);
                return;
            }
            catch (BufferedWriteException) when (attempt < _attempts) { resumeFlush = true; }
            catch (IOException) when (attempt < _attempts) { }
        }
    }
    public override void Flush()
    {
        CheckDisposed();
        for (var attempt = 1; ; attempt++)
        {
            try { base.Flush(); return; }
            catch (IOException) when (attempt < _attempts) { }
        }
    }
}
