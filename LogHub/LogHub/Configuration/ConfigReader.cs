using System;
using System.Collections.Generic;
using System.IO;

namespace LogHub.Configuration;
public class ConfigReader
{
    public LogConfiguration Load(string path)
    {
        var values = new Dictionary<string, string>
        {
            ["sink"] = "console",
            ["minSeverity"] = "Info",
            ["format"] = "plain",
            ["connection"] = "local"
        };
        if (!File.Exists(path))
        {
            return CreateConfiguration(values);
        }
        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                continue;
            }
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                values[parts[0].Trim()] = parts[1].Trim();
            }
        }
        return CreateConfiguration(values);
    }
    private static LogConfiguration CreateConfiguration(
        Dictionary<string, string> values)
    {
        return new LogConfiguration
        {
            Sink = values["sink"],
            MinSeverity = values["minSeverity"],
            Format = values["format"],
            Connection = values["connection"]
        };
    }
}