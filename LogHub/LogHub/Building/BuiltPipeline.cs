using LogHub.Application;
using LogHub.Processing;
namespace LogHub.Building;
public sealed class BuiltPipeline : IDisposable
{
    private readonly LogHubApplication _application;
    private readonly IDisposable _lifetime;
    private bool _disposed;
    internal BuiltPipeline(LogHubApplication application, IDisposable lifetime)
        => (_application, _lifetime) = (application, lifetime);
    public ProcessingResult Run(bool showGraph = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _application.Run(showGraph);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _lifetime.Dispose();
    }
}
