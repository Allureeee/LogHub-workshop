using LogHub.Importing;
namespace LogHub.Processing;
public sealed class LogFlowMediator : IDisposable
{
    private readonly ImportSession _importer;
    private readonly ProcessingPipeline _pipeline;
    private bool _running;
    private bool _disposed;
    public int PauseCount { get; private set; }
    public int ResumeCount { get; private set; }
    public LogFlowMediator(ImportSession importer, ProcessingPipeline pipeline)
    {
        _importer = importer; _pipeline = pipeline;
        _pipeline.PressureChanged += OnPressureChanged;
    }
    private void OnPressureChanged(bool overloaded)
    {
        if (overloaded && !_importer.Paused) { _importer.Pause(); PauseCount++; }
        else if (!overloaded && _importer.Paused) { _importer.Resume(); ResumeCount++; }
    }
    public void ChangeImporter(ILogImporter importer) => _importer.ChangeImporter(importer);
    public ProcessingResult Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_running) throw new InvalidOperationException("Concurrent/re-entrant runs are not supported.");
        _running = true;
        PauseCount = ResumeCount = 0;
        try
        {
            _pipeline.Reset();
            _importer.Start();
            while (true)
            {
                if (_importer.Paused) { _pipeline.Drain(); continue; }
                if (!_importer.TryRead(out var item)) break;
                _pipeline.Enqueue(item!);
            }
            _pipeline.Drain();
            return _pipeline.Result;
        }
        finally
        {
            try { _importer.Stop(); }
            finally { _pipeline.ClearPending(); _running = false; }
        }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _pipeline.PressureChanged -= OnPressureChanged;
        _importer.Stop();
        _disposed = true;
    }
}
