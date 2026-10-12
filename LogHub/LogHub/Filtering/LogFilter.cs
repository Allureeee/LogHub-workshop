using LogHub.Models;
namespace LogHub.Filtering;

public abstract class LogFilter
{
    private LogFilter? _next;
    public abstract string Name { get; }
    internal void SetNext(LogFilter? next) => _next = next;
    public bool Handle(LogRecord entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return Allows(entry) && (_next?.Handle(entry) ?? true);
    }
    protected abstract bool Allows(LogRecord entry);
    public void Reset()
    {
        ResetState();
        _next?.Reset();
    }
    protected virtual void ResetState() { }
}
