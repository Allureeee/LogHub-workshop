namespace LogHub.Importing.Sources;
public sealed class FileTextSource : ITextSource
{
    private readonly string _path;
    public FileTextSource(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }
    public TextReader Open() => new StreamReader(_path);
}
