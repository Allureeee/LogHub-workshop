using System.Collections.Concurrent;
using LogHub.Catalog;
namespace LogHub.Proxy;
public sealed class CachingSeverityCatalog : ISeverityCatalog
{
    private readonly ISeverityCatalog _source;
    private readonly ConcurrentDictionary<string, Lazy<int>> _cache = new(StringComparer.OrdinalIgnoreCase);
    public CachingSeverityCatalog(ISeverityCatalog source) => _source = source ?? throw new ArgumentNullException(nameof(source));
    public int GetCode(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _cache.GetOrAdd(name, key => new Lazy<int>(() => _source.GetCode(key))).Value;
    }
    public bool Invalidate(string name) => _cache.TryRemove(name, out _);
    public void Clear() => _cache.Clear();
}
