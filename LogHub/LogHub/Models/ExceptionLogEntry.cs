using LogHub.Visiting;
namespace LogHub.Models;

public sealed class ExceptionLogEntry : LogRecord
{
    public string StackTrace { get; init; } = string.Empty;
    public override void Accept(ILogEntryVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.Visit(this);
    }
}
