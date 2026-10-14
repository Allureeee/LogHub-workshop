using LogHub.Models;
using LogHub.Rules;
namespace LogHub.Filtering;
public sealed class CompositeRuleFilter : LogFilter
{
    private readonly ILogRule _rule;
    public CompositeRuleFilter(ILogRule rule) => _rule = rule;
    public override string Name => "rule";
    protected override bool Allows(LogRecord entry) => _rule.Matches(entry);
}
