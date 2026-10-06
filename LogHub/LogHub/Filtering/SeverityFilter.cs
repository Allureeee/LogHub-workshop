using System.Collections.Generic;
using LogHub.Models;

namespace LogHub.Filtering;
public class SeverityFilter
{
    private static readonly Dictionary<string, int> SeverityLevels = new()
    {
        ["Debug"] = 0,
        ["Info"] = 1,
        ["Warning"] = 2,
        ["Error"] = 3,
        ["Critical"] = 4
    };
    private readonly int _minimumSeverity;
    public SeverityFilter(string minimumSeverity)
    {
        _minimumSeverity = GetSeverityValue(minimumSeverity);
    }
    public bool ShouldInclude(LogRecord record)
    {
        return GetSeverityValue(record.Severity) >= _minimumSeverity;
    }
    private static int GetSeverityValue(string severity)
    {
        return SeverityLevels.GetValueOrDefault(severity, 1);
    }
}