namespace LogHub.Observing;
public abstract class LogObserver : IPushLogObserver, IPullLogObserver
{
    public void OnProcessed(ProcessedLog item) => Observe(item);
    public void OnChanged(IPullLogSource source) => Observe(source.Current
        ?? throw new InvalidOperationException("The source has no current record."));
    protected abstract void Observe(ProcessedLog item);
    public virtual void Reset() { }
}
