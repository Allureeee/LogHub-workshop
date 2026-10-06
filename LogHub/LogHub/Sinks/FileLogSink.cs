using LogHub.Configuration;
using LogHub.Models;

namespace LogHub.Sinks;
[SinkName("file")]
public class FileLogSink : ILogSink
{
    private readonly StreamWriter _writer;
    public FileLogSink(LogConfiguration configuration)
    {
        var fileName = $"logs-{DateTime.Now:yyyyMMdd}.txt";
        _writer = new StreamWriter(fileName, append: true);
    }
    public string Name => "file";
    public void Write(LogRecord record, string text)
    {
        _writer.WriteLine(text);
        _writer.Flush();
    }
    public void Dispose()
    {
        _writer.Dispose();
    }
}