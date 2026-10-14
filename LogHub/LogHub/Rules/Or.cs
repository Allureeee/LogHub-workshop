using LogHub.Models;
namespace LogHub.Rules;
public sealed class Or : ILogRule
{
    private readonly ILogRule[] _children;
    public Or(params ILogRule[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        if (children.Any(c => c is null)) throw new ArgumentException("A child rule is null.", nameof(children));
        _children = children.ToArray();
    }
    public bool Matches(LogRecord record) => _children.Any(r => r.Matches(record));
}
