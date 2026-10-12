using LogHub.Visiting;
namespace LogHub.Models;

public sealed class MetricLogEntry : LogRecord
{
    public double Value { get; init; }
    public override void Accept(ILogEntryVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.Visit(this);
    }
}
