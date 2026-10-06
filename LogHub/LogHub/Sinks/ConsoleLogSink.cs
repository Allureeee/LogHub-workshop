using LogHub.Configuration;
using LogHub.Models;

namespace LogHub.Sinks;
[SinkName("console")]
public class ConsoleLogSink : ILogSink
{
    public ConsoleLogSink(LogConfiguration configuration)
    {
    }
    public string Name => "console";
    public void Write(LogRecord record, string text)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor =
            record.Severity == "Error" || record.Severity == "Critical"
                ? ConsoleColor.Red
                : ConsoleColor.Gray;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
    public void Dispose()
    {
    }
}