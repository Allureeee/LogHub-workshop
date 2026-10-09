using System;
using System.Collections.Generic;

namespace LogHub.Configuration;
public sealed class LazyConfigService : IConfigService
{
    private static readonly Lazy<LazyConfigService> _instance =
        new(() => new LazyConfigService());
    private readonly Dictionary<string, string> _values = new();
    private LazyConfigService()
    {
    }
    public static LazyConfigService Instance()
    {
        return _instance.Value;
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