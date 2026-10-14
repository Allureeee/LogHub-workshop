using System.Text;
using System.Text.Json;
using LogHub.External;
namespace LogHub.Infrastructure;
public sealed class JsonFileChannel : IDocumentChannel
{
    private readonly StreamWriter _writer;
    public JsonFileChannel(string path)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        _writer = new StreamWriter(fullPath, append: true, new UTF8Encoding(false));
    }
    public void Send(JsonElement document) { _writer.WriteLine(document.GetRawText()); _writer.Flush(); }
    public void Dispose() => _writer.Dispose();
}
