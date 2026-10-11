using System.Text;
using LogHub.Configuration;
using LogHub.Models;
namespace LogHub.Sinks;
[SinkName("file")]
public class FileLogSink : ILogSink
{
    private readonly StreamWriter _writer;
    public FileLogSink(LogConfiguration configuration)
    {
        var fileName = string.IsNullOrWhiteSpace(configuration.OutputPath)
            ? $"logs-{DateTime.Now:yyyyMMdd}.txt" : configuration.OutputPath;
        var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
        if (directory is not null) Directory.CreateDirectory(directory);
        _writer = new StreamWriter(fileName, append: true, new UTF8Encoding(false));
    }
    public string Name => "file";
    public void Write(LogRecord record, string text)
    {
        _writer.WriteLine(text);
        _writer.Flush();
    }
    public void Dispose() => _writer.Dispose();
}
