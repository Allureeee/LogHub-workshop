using LogHub.Models;
namespace LogHub.Rules;
public sealed class MessageContains : ILogRule
{
    private readonly string _value;
    public MessageContains(string value)
    { ArgumentException.ThrowIfNullOrWhiteSpace(value); _value = value; }
    public bool Matches(LogRecord record) => record.Message.Contains(_value, StringComparison.OrdinalIgnoreCase);
}
