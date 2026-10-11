namespace LogHub.Importing.Sources;

public sealed class InMemoryTextSource : ITextSource
{
    private readonly string _text;
    public InMemoryTextSource(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
    }
    public TextReader Open() => new StringReader(_text);
}
