using LogHub.Configuration;
namespace LogHub.Observing;
public sealed class PipelineSubscriptions : IDisposable
{
    private readonly LogObserver[] _observers;
    private readonly IDisposable[] _tokens;
    public PipelineSubscriptions(PipelineEvents events, LogConfiguration configuration,
        IEnumerable<LogObserver> observers)
    {
        _observers = observers.ToArray();
        var mode = configuration.Notifications.ToLowerInvariant();
        if (mode != "push" && mode != "pull")
            throw new NotSupportedException($"Unknown notification model '{mode}'.");
        _tokens = _observers.Select(observer => mode == "push"
            ? events.SubscribePush(observer) : events.SubscribePull(observer)).ToArray();
    }
    public void Reset()
    {
        foreach (var observer in _observers) observer.Reset();
    }
    public void Dispose()
    {
        foreach (var token in _tokens) token.Dispose();
    }
}
