using LogHub.Models;
using LogHub.Sinks;
using LogHub.Decorating;
namespace LogHub.Families;
public sealed class FamilyFormattedSink : LogSinkDecorator
{
    private readonly FamilyFormatter _formatter;
    public FamilyFormattedSink(ILogSink inner, FamilyFormatter formatter) : base(inner) => _formatter = formatter;
    public override void Write(LogRecord record, string text)
    { CheckDisposed(); Inner.Write(record, _formatter.Format(record).Text); }
}
