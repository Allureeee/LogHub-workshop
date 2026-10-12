using LogHub.Configuration;
using LogHub.Models;
namespace LogHub.Filtering;

public sealed class FilterChain
{
    private readonly LogFilter? _first;
    public IReadOnlyList<string> Names { get; }
    public FilterChain(LogConfiguration configuration, IEnumerable<LogFilter> available)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(available);
        var byName = available.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        var names = configuration.Filters.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Length)
            throw new ArgumentException("A filter cannot occur twice in the same chain.");
        var links = names.Select(name => byName.TryGetValue(name, out var filter) ? filter
            : throw new NotSupportedException($"Unknown filter '{name}'.")).ToArray();
        for (var i = 0; i < links.Length; i++) links[i].SetNext(i + 1 < links.Length ? links[i + 1] : null);
        _first = links.FirstOrDefault();
        Names = Array.AsReadOnly(names);
    }
    public bool Handle(LogRecord entry) => _first?.Handle(entry) ?? true;
    public void Reset() => _first?.Reset();
}
