using LogHub.Models;
using LogHub.Visiting;
namespace LogHub.Filtering;

public sealed class DuplicateLogFilter : LogFilter
{
    private readonly DuplicateKeyVisitor _keyVisitor;
    private readonly HashSet<DuplicateKey> _seen = new();
    public DuplicateLogFilter(DuplicateKeyVisitor keyVisitor) => _keyVisitor = keyVisitor;
    public override string Name => "duplicates";
    protected override bool Allows(LogRecord entry)
    {
        entry.Accept(_keyVisitor);
        return _seen.Add(_keyVisitor.Key);
    }
    protected override void ResetState() => _seen.Clear();
}
