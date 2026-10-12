namespace LogHub.Observing;
public sealed class PipelineEvents : IPullLogSource
{
    private readonly List<Subscription> _subscriptions = new();
    private readonly List<ObserverFailure> _failures = new();
    private bool _publishing;
    public ProcessedLog? Current { get; private set; }
    public IReadOnlyList<ObserverFailure> Failures => _failures.AsReadOnly();
    public IDisposable SubscribePush(IPushLogObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return Add(observer.GetType().Name, observer.OnProcessed);
    }
    public IDisposable SubscribePull(IPullLogObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return Add(observer.GetType().Name, _ => observer.OnChanged(this));
    }
    private IDisposable Add(string name, Action<ProcessedLog> callback)
    {
        var subscription = new Subscription(this, name, callback);
        _subscriptions.Add(subscription);
        return subscription;
    }
    public void Publish(ProcessedLog item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_publishing) throw new InvalidOperationException("Re-entrant publication is not supported.");
        _publishing = true;
        Current = item;
        try
        {
            foreach (var subscription in _subscriptions.ToArray())
            {
                try { subscription.Deliver(item); }
                catch (Exception error) { _failures.Add(new ObserverFailure(subscription.Name, error)); }
            }
        }
        finally { _publishing = false; }
    }
    public void Reset()
    {
        if (_publishing) throw new InvalidOperationException("Cannot reset during publication.");
        Current = null;
        _failures.Clear();
    }
    private sealed class Subscription : IDisposable
    {
        private PipelineEvents? _owner;
        private Action<ProcessedLog>? _callback;
        public string Name { get; }
        public Subscription(PipelineEvents owner, string name, Action<ProcessedLog> callback)
            => (_owner, Name, _callback) = (owner, name, callback);
        public void Deliver(ProcessedLog item) => _callback?.Invoke(item);
        public void Dispose()
        {
            _owner?._subscriptions.Remove(this);
            _owner = null;
            _callback = null;
        }
    }
}
