using System.Text.Json;
namespace LogHub.External;
public interface IDocumentChannel : IDisposable
{
    void Send(JsonElement document);
}
