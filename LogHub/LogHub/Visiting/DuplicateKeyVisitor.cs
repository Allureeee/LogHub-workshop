using System.Globalization;
using LogHub.Models;
namespace LogHub.Visiting;
public readonly record struct DuplicateKey(string Kind, string Severity, string Message, string Detail);

public sealed class DuplicateKeyVisitor : ILogEntryVisitor
{
    public DuplicateKey Key { get; private set; }
    public void Visit(SimpleLogEntry entry) => Key = Make(entry, "simple", string.Empty);
    public void Visit(ExceptionLogEntry entry) => Key = Make(entry, "exception", entry.StackTrace);
    public void Visit(MetricLogEntry entry) =>
        Key = Make(entry, "metric", entry.Value.ToString("R", CultureInfo.InvariantCulture));
    private static DuplicateKey Make(LogRecord entry, string kind, string detail) =>
        new(kind, entry.Severity.ToUpperInvariant(), entry.Message, detail);
}
