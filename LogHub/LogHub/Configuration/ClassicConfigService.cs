using System.Collections.Generic;

namespace LogHub.Configuration;
public sealed class ClassicConfigService : IConfigService
{
    private static ClassicConfigService? _instance;
    private readonly Dictionary<string, string> _values = new();
    private ClassicConfigService()
    {
    }
    public static ClassicConfigService Instance()
    {
        return _instance ??= new ClassicConfigService();
    }
    public string? GetValue(string key)
    {
        return _values.GetValueOrDefault(key);
    }
    public void SetValue(string key, string value)
    {
        _values[key] = value;
    }
}