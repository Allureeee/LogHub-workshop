using System;
using System.Globalization;
using LogHub.Models;

namespace LogHub.Parsing;
public class LogParser
{
    public LogRecord? Parse(string line)
    {
        var parts = line.Split('|', 3);

        if (parts.Length != 3)
        {
            return null;
        }
        if (!DateTime.TryParse(
                parts[0],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return null;
        }
        return new LogRecord
        {
            Date = date,
            Severity = parts[1].Trim(),
            Message = parts[2].Trim()
        };
    }
}