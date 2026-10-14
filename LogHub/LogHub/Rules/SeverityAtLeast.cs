using LogHub.Catalog;
using LogHub.Models;
namespace LogHub.Rules;
public sealed class SeverityAtLeast : ILogRule
{
    private readonly ISeverityCatalog _catalog;
    private readonly int _minimum;
    public SeverityAtLeast(string minimum, ISeverityCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _minimum = catalog.GetCode(minimum);
    }
    public bool Matches(LogRecord record) => _catalog.GetCode(record.Severity) >= _minimum;
}
