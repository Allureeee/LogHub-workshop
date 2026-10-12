namespace LogHub.Observing;

public sealed class ProcessedCounterObserver : LogObserver
{
    public int Count { get; private set; }
    protected override void Observe(ProcessedLog item) => Count++;
    public override void Reset() => Count = 0;
}
