namespace LogHub.Configuration;
public class ConfigReader
{
    public LogConfiguration Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return Parse(File.ReadLines(fullPath), Path.GetDirectoryName(fullPath)!);
    }
    public LogConfiguration Parse(IEnumerable<string> lines, string baseDirectory)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sink"] = "console,file", ["importer"] = "mixed",
            ["minSeverity"] = "Info", ["format"] = "plain", ["connection"] = "local",
            ["logPath"] = "sample.log", ["outputPath"] = string.Empty
        };
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var parts = line.Split('=', 2);
            if (parts.Length == 2) values[parts[0].Trim()] = parts[1].Trim();
        }
        return new LogConfiguration
        {
            Sink = values["sink"], Importer = values["importer"],
            MinSeverity = values["minSeverity"], Format = values["format"],
            Connection = values["connection"],
            LogPath = Path.GetFullPath(values["logPath"], baseDirectory),
            OutputPath = string.IsNullOrWhiteSpace(values["outputPath"])
                ? string.Empty : Path.GetFullPath(values["outputPath"], baseDirectory)
        };
    }
}
