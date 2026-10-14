using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Decorating;
public sealed class BufferedLogSink : LogSinkDecorator
{
    private readonly Queue<(LogRecord Record, string Text)> _pending = new();
    private readonly int _size;
    public int PendingCount => _pending.Count;
    public BufferedLogSink(ILogSink inner, int size) : base(inner)
    {
        if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));
        _size = size;
    }
    public override void Write(LogRecord record, string text)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(record);
        if (_pending.Count >= _size) Flush();
        _pending.Enqueue((record, text));
        if (_pending.Count < _size) return;
        try { Flush(); }
        catch (IOException error) { throw new BufferedWriteException(error); }
    }
    public override void Flush()
    {
        CheckDisposed();
        while (_pending.TryPeek(out var item))
        {
            Inner.Write(item.Record, item.Text);
            _pending.Dequeue();
        }
        base.Flush();
    }
}
