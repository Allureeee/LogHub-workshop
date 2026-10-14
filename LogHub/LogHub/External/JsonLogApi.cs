using System.Text.Json;
namespace LogHub.External;
public class JsonLogApi : IDisposable
{
    private readonly IDocumentChannel _channel;
    public JsonLogApi(IDocumentChannel channel) => _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    public void Push(JsonDocument document) => _channel.Send(document.RootElement);
    public void Dispose() => _channel.Dispose();
}
