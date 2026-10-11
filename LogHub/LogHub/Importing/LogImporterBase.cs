using LogHub.Importing.Sources;
using LogHub.Models;
namespace LogHub.Importing;

public abstract class LogImporterBase : ILogImporter
{
    private readonly ITextSource _source;
    protected LogImporterBase(ITextSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }
    public IEnumerable<ImportItem> Import()
    {
        var reader = Open();
        try
        {
            var lineNumber = 0;
            string? line;
            while ((line = Read(reader)) is not null)
            {
                lineNumber++;
                var record = Parse(line);
                if (record is not null && !Validate(record))
                    record = null;
                yield return new ImportItem(lineNumber, record);
            }
        }
        finally
        {
            try { Close(reader); }
            finally { Release(reader); }
        }
    }

    protected virtual TextReader Open() => _source.Open();
    protected abstract string? Read(TextReader reader);
    protected abstract LogRecord? Parse(string line);
    protected virtual bool Validate(LogRecord record) => true;
    protected virtual void Close(TextReader reader) => reader.Close();
    protected virtual void Release(TextReader reader) => reader.Dispose();
}
