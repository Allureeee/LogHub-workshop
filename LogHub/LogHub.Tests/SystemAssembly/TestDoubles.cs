using System.Text.Json;
using LogHub.Catalog;
using LogHub.Configuration;
using LogHub.External;
using LogHub.Importing.Sources;
using LogHub.Infrastructure;
using LogHub.Models;
using LogHub.Sinks;
using LogHub.Tests.Pipeline;
namespace LogHub.Tests.SystemAssembly;

internal sealed class ManualClock : TimeProvider
{
    private long _ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => _ticks;
    public override DateTimeOffset GetUtcNow() => new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);
    public void Advance(TimeSpan amount) => _ticks += amount.Ticks;
}
internal sealed class RecordingSink : ILogSink
{
    public string Name { get; }
    public List<LogRecord> Records { get; } = new();
    public List<string> Texts { get; } = new();
    public int Attempts { get; private set; }
    public int RemainingFailures { get; set; }
    public int FailOnAttempt { get; set; } = -1;
    public bool FailAlways { get; set; }
    public bool ThrowArgument { get; set; }
    public bool ThrowOnDispose { get; set; }
    public int DisposeCount { get; private set; }
    public Action? OnWrite { get; set; }
    public RecordingSink(string name = "recording") => Name = name;
    public void Write(LogRecord record, string text)
    {
        ObjectDisposedException.ThrowIf(DisposeCount != 0, this);
        Attempts++; OnWrite?.Invoke();
        if (ThrowArgument) throw new ArgumentException("Permanent validation error.");
        if (FailAlways || Attempts == FailOnAttempt) throw new IOException("Temporary output failure.");
        if (RemainingFailures > 0) { RemainingFailures--; throw new IOException("Temporary output failure."); }
        Records.Add(record); Texts.Add(text);
    }
    public void Dispose()
    {
        if (DisposeCount != 0) return;
        DisposeCount++;
        if (ThrowOnDispose) throw new IOException("Close failed.");
    }
}
internal sealed class RecordingSqlSession : ISqlSession
{
    public List<(DateTime Date, int Severity, string Message)> Rows { get; } = new();
    public int OpenCount { get; private set; }
    public int DisposeCount { get; private set; }
    public void Open() => OpenCount++;
    public void Save(DateTime date, int severity, string message)
    {
        if (OpenCount != 1 || DisposeCount != 0) throw new InvalidOperationException("Invalid session lifetime.");
        Rows.Add((date, severity, message));
    }
    public void Dispose() { if (DisposeCount == 0) DisposeCount++; }
}
internal sealed class RecordingDocumentChannel : IDocumentChannel
{
    public List<JsonElement> Documents { get; } = new();
    public int DisposeCount { get; private set; }
    public void Send(JsonElement document) => Documents.Add(document.Clone());
    public void Dispose() { if (DisposeCount == 0) DisposeCount++; }
}
internal sealed class MemoryResources : IHubResources
{
    public string Input { get; set; } = Variant20PipelineFixture.Text;
    public TextWriter Output { get; } = new StringWriter();
    public TimeProvider Clock { get; } = new ManualClock();
    public RecordingSink ConsoleSink { get; } = new("console");
    public RecordingSink FileSink { get; } = new("file");
    public List<RecordingSqlSession> Sessions { get; } = new();
    public List<RecordingDocumentChannel> Channels { get; } = new();
    public int FileCreations { get; private set; }
    public ITextSource CreateSource(string path) => new InMemoryTextSource(Input);
    public ILogSink CreateConsole(LogConfiguration configuration) => ConsoleSink;
    public ILogSink CreateFile(LogConfiguration configuration) { FileCreations++; return FileSink; }
    public ISqlSession CreateSqlSession(string path)
    { var session = new RecordingSqlSession(); Sessions.Add(session); return session; }
    public IDocumentChannel CreateDocumentChannel(string path)
    { var channel = new RecordingDocumentChannel(); Channels.Add(channel); return channel; }
}
internal sealed class CountingCatalog : ISeverityCatalog
{
    private int _calls;
    public int Calls => _calls;
    public int GetCode(string name) { Interlocked.Increment(ref _calls); return name.Equals("Error", StringComparison.OrdinalIgnoreCase) ? 3 : 1; }
}
internal static class Samples
{
    public const string TwoFormats = "2026-10-07T11:30:00|Info|pipe\n" +
        "{\"date\":\"2026-10-07T11:30:01Z\",\"level\":\"Error\",\"message\":\"json\"}\n";
    public static LogRecord Record(string message = "event", string level = "Info") => new SimpleLogEntry
    { Date = new DateTime(2026, 10, 7, 11, 30, 0, DateTimeKind.Utc), Severity = level, Message = message };
    public static LogConfiguration Profile(string family = "development", string builder = "fluent", string adapter = "object")
    {
        var profile = ProfileCatalog.Default();
        profile.Family = family; profile.Builder = builder; profile.Adapter = adapter;
        return profile;
    }
}
