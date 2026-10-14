using LogHub.Formatting;
using LogHub.Models;
namespace LogHub.Families;
public sealed class CompactFamilyFormatter : FamilyFormatter
{
    private readonly LogFormatter _visitor;
    public CompactFamilyFormatter(LogFormatter visitor) => _visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
    public override FamilyKind Family => FamilyKind.Production;
    public override FamilyPacket Format(LogRecord record) => new(Family, record, _visitor.Format(record));
}
