namespace LogHub.Observing;

public sealed class CriticalAlertObserver : LogObserver
{
    private readonly TextWriter _output;
    public int AlertCount { get; private set; }
    public CriticalAlertObserver(TextWriter output) => _output = output;
    protected override void Observe(ProcessedLog item)
    {
        if (!item.Record.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase)) return;
        AlertCount++;
        _output.WriteLine($"[ALERT Critical] #{item.Sequence}: {item.Text}");
    }
    public override void Reset() => AlertCount = 0;
}
