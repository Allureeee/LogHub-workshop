using LogHub.Models;
namespace LogHub.Rules;
public sealed class Not : ILogRule
{
    private readonly ILogRule _child;
    public Not(ILogRule child) => _child = child ?? throw new ArgumentNullException(nameof(child));
    public bool Matches(LogRecord record) => !_child.Matches(record);
}
