using LogHub.Configuration;
using LogHub.Models;

namespace LogHub.Sinks;
[SinkName("database")]
public class DatabaseLogSink : ILogSink
{
    private readonly string _connectionString;
    private bool _open;
    public DatabaseLogSink(LogConfiguration configuration)
    {
        _connectionString = configuration.Connection;
        Open();
    }
    public string Name => "database";
    public void Write(LogRecord record, string text)
    {
        if (!_open)
        {
            throw new InvalidOperationException(
                "Соединение с базой данных не открыто");
        }
        Console.WriteLine(
            $"[БД] INSERT INTO logs(date, severity, message) VALUES " +
            $"(@d, @s, @m) -> {record.Date:O}, {record.Severity}, {text}");
    }
    private void Open()
    {
        _open = true;
        Console.WriteLine(
            $"[БД] соединение открыто: {_connectionString}");
    }
    public void Dispose()
    {
        if (!_open)
        {
            return;
        }
        _open = false;
        Console.WriteLine("[БД] соединение закрыто");
    }
}