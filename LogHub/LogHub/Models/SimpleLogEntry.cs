using LogHub.Visiting;
namespace LogHub.Models;

public sealed class SimpleLogEntry : LogRecord
{
    public override void Accept(ILogEntryVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.Visit(this);
    }
}
