using System.Globalization;
using Microsoft.Data.Sqlite;
using LogHub.External;
namespace LogHub.Infrastructure;
public sealed class SqliteLogSession : ISqlSession
{
    private readonly string _path;
    private SqliteConnection? _connection;
    private bool _disposed;
    public SqliteLogSession(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }
    public void Open()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connection is not null) return;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = _path, Pooling = false }.ToString());
        try
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS logs (id INTEGER PRIMARY KEY AUTOINCREMENT, date TEXT NOT NULL, severity INTEGER NOT NULL, message TEXT NOT NULL)";
            command.ExecuteNonQuery();
            _connection = connection;
        }
        catch { connection.Dispose(); throw; }
    }
    public void Save(DateTime date, int severity, string message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var connection = _connection ?? throw new InvalidOperationException("Open the database before saving.");
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO logs(date,severity,message) VALUES ($date,$severity,$message)";
        command.Parameters.AddWithValue("$date", date.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$severity", severity);
        command.Parameters.AddWithValue("$message", message);
        try { command.ExecuteNonQuery(); }
        catch (SqliteException e) when (e.SqliteErrorCode is 5 or 6)
        { throw new IOException("SQLite is temporarily busy or locked.", e); }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection?.Dispose();
    }
}
