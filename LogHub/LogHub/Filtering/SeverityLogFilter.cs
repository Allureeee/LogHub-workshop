using LogHub.Models;
namespace LogHub.Filtering;

public sealed class SeverityLogFilter : LogFilter
{
    private readonly SeverityFilter _predicate;
    public SeverityLogFilter(SeverityFilter predicate) => _predicate = predicate;
    public override string Name => "severity";
    protected override bool Allows(LogRecord entry) => _predicate.ShouldInclude(entry);
}
