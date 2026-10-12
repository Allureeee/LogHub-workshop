using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Models;
using LogHub.Observing;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;
using LogHub.Visiting;
namespace LogHub.Tests;
internal static class TestPipeline
{
    public static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);
    public static SimpleLogEntry Entry(string message = "entry", string severity = "Info", int ageMinutes = 5) =>
        new() { Date = Now.AddMinutes(-ageMinutes).UtcDateTime, Severity = severity, Message = message };
    public static LogProcessor CreateProcessor(ILogImporter importer, SeverityFilter filter,
        LogFormatter formatter, params ILogSink[] sinks)
    {
        if (sinks.Length == 0) throw new ArgumentException("At least one sink is required.");
        var configuration = new LogConfiguration { Filters = "severity" };
        var chain = new FilterChain(configuration, new[] { new SeverityLogFilter(filter) });
        var events = new PipelineEvents();
        var subscriptions = new PipelineSubscriptions(events, configuration,
            sinks.Select(s => new SinkObserver<ILogSink>(s)));
        var pipeline = new ProcessingPipeline(configuration, chain, formatter,
            new StatisticsVisitor(), events, subscriptions);
        return new LogProcessor(new LogFlowMediator(new ImportSession(importer), pipeline));
    }
    public static T Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T exception) { return exception; }
        Assert.Fail($"Expected {typeof(T).Name}.");
        throw new InvalidOperationException("Unreachable assertion continuation.");
    }
}

internal sealed class MemoryLogSink : ILogSink
{
    public string Name => "memory";
    public bool Fail { get; init; }
    public List<LogRecord> Records { get; } = new();
    public List<string> Texts { get; } = new();
    public void Write(LogRecord record, string text)
    {
        if (Fail) throw new IOException("Injected receiver failure.");
        Records.Add(record); Texts.Add(text);
    }
    public void Dispose() { }
}

internal sealed class PipelineRig : IDisposable
{
    public PipelineEvents Events { get; } = new();
    public StatisticsVisitor Statistics { get; } = new();
    public ProcessedCounterObserver Counter { get; } = new();
    public CriticalAlertObserver Alerts { get; }
    public StringWriter AlertText { get; } = new();
    public MemoryLogSink Sink { get; } = new();
    public ImportSession Session { get; }
    public ProcessingPipeline Pipeline { get; }
    public LogFlowMediator Mediator { get; }
    public LogProcessor Processor { get; }
    private readonly PipelineSubscriptions _subscriptions;
    public PipelineRig(string text, LogConfiguration? configuration = null, ILogImporter? importer = null,
        IEnumerable<LogObserver>? additionalObservers = null)
    {
        configuration ??= new LogConfiguration();
        var chain = new FilterChain(configuration, new LogFilter[]
        {
            new DuplicateLogFilter(new DuplicateKeyVisitor()),
            new AgeLogFilter(configuration, new FixedTimeProvider(TestPipeline.Now)),
            new SeverityLogFilter(new SeverityFilter(configuration.MinSeverity))
        });
        Alerts = new CriticalAlertObserver(AlertText);
        var observers = new List<LogObserver> { Counter, Alerts, new SinkObserver<MemoryLogSink>(Sink) };
        if (additionalObservers is not null) observers.AddRange(additionalObservers);
        _subscriptions = new PipelineSubscriptions(Events, configuration, observers);
        Pipeline = new ProcessingPipeline(configuration, chain, new LogFormatter(configuration.Format),
            Statistics, Events, _subscriptions);
        Session = new ImportSession(importer ?? new MixedLogImporter(new InMemoryTextSource(text), new LogParser()));
        Mediator = new LogFlowMediator(Session, Pipeline);
        Processor = new LogProcessor(Mediator);
    }
    public void Dispose()
    {
        Mediator.Dispose(); Session.Dispose(); _subscriptions.Dispose(); Sink.Dispose(); AlertText.Dispose();
    }
}
