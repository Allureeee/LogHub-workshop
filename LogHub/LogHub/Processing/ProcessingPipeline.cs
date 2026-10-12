using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Observing;
using LogHub.Visiting;
namespace LogHub.Processing;

public sealed class ProcessingPipeline
{
    private readonly Queue<ImportItem> _buffer = new();
    private readonly FilterChain _chain;
    private readonly LogFormatter _formatter;
    private readonly StatisticsVisitor _statistics;
    private readonly PipelineEvents _events;
    private readonly PipelineSubscriptions _subscriptions;
    private int _processed;
    private int _skipped;
    public int Capacity { get; }
    public int PendingCount => _buffer.Count;
    public int MaximumBuffered { get; private set; }
    public event Action<bool>? PressureChanged;
    public ProcessingResult Result => new(_processed, _skipped, _events.Failures.Count);

    public ProcessingPipeline(LogConfiguration configuration, FilterChain chain, LogFormatter formatter,
        StatisticsVisitor statistics, PipelineEvents events, PipelineSubscriptions subscriptions)
    {
        if (configuration.BufferCapacity < 1) throw new ArgumentOutOfRangeException(nameof(configuration),
            "bufferCapacity must be at least 1.");
        Capacity = configuration.BufferCapacity;
        _chain = chain; _formatter = formatter; _statistics = statistics;
        _events = events; _subscriptions = subscriptions;
    }
    public void Reset()
    {
        if (_buffer.Count != 0) throw new InvalidOperationException("Drain or clear the buffer first.");
        _processed = _skipped = MaximumBuffered = 0;
        _chain.Reset(); _statistics.Reset(); _events.Reset(); _subscriptions.Reset();
    }
    public void Enqueue(ImportItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_buffer.Count >= Capacity) throw new InvalidOperationException("The pipeline buffer is full.");
        _buffer.Enqueue(item);
        MaximumBuffered = Math.Max(MaximumBuffered, _buffer.Count);
        if (_buffer.Count == Capacity) PressureChanged?.Invoke(true);
    }
    public void Drain()
    {
        while (_buffer.TryDequeue(out var item))
        {
            var entry = item.Record;
            if (entry is null || !_chain.Handle(entry)) { _skipped++; continue; }
            var text = _formatter.Format(entry); 
            entry.Accept(_statistics);
            _processed++;
            _events.Publish(new ProcessedLog(_processed, entry, text));
        }
        PressureChanged?.Invoke(false);
    }
    public void ClearPending()
    {
        _buffer.Clear();
        PressureChanged?.Invoke(false);
    }
}
