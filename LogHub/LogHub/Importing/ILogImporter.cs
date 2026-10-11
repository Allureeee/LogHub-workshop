using LogHub.Models;
namespace LogHub.Importing;

public interface ILogImporter
{
    IEnumerable<ImportItem> Import();
}
public sealed record ImportItem(int LineNumber, LogRecord? Record);
