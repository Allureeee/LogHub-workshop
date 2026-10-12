namespace LogHub.Importing;
public sealed class ImportSession : IDisposable
{
    private ILogImporter _strategy;
    private IEnumerator<ImportItem>? _iterator;
    public bool Paused { get; private set; }
    public int ReadCount { get; private set; }
    public ImportSession(ILogImporter strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategy = strategy;
    }
    public void ChangeImporter(ILogImporter strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        if (_iterator is not null) throw new InvalidOperationException("Change the importer between runs.");
        _strategy = strategy;
    }
    public void Start()
    {
        if (_iterator is not null) throw new InvalidOperationException("The import session is already active.");
        Paused = false;
        ReadCount = 0;
        _iterator = _strategy.Import().GetEnumerator();
    }
    public bool TryRead(out ImportItem? item)
    {
        item = null;
        if (_iterator is null) throw new InvalidOperationException("Start the import session first.");
        if (Paused || !_iterator.MoveNext()) return false;
        item = _iterator.Current;
        ReadCount++;
        return true;
    }
    public void Pause() => Paused = true;
    public void Resume() => Paused = false;
    public void Stop()
    {
        var iterator = _iterator;
        _iterator = null;
        Paused = false;
        iterator?.Dispose();
    }
    public void Dispose() => Stop();
}
