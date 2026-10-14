using LogHub.Formatting;
using LogHub.Models;
namespace LogHub.Families;
public sealed class DetailedFamilyFormatter : FamilyFormatter
{
    private readonly LogFormatter _visitor;
    public DetailedFamilyFormatter(LogFormatter visitor) => _visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
    public override FamilyKind Family => FamilyKind.Development;
    public override FamilyPacket Format(LogRecord record) => new(Family, record, _visitor.Format(record));
}
