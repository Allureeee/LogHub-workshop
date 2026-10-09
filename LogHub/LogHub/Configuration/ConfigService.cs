using System.Collections.Generic;

namespace LogHub.Configuration;
public class ConfigService : IConfigService
{
    private readonly Dictionary<string, string> _values;
    public ConfigService(LogConfiguration configuration)
    {
        _values = new Dictionary<string, string>
        {
            ["sink"] = configuration.Sink,
            ["minSeverity"] = configuration.MinSeverity,
            ["format"] = configuration.Format,
            ["connection"] = configuration.Connection,
            ["logPath"] = configuration.LogPath
        };
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