using LogHub.Sinks;
namespace LogHub.Families;
public sealed class DatabaseFamilyStorage : FamilyStorage
{
    private readonly ILogSink _sink;
    public DatabaseFamilyStorage(ILogSink sink) => _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    public override FamilyKind Family => FamilyKind.Production;
    public override string Name => "database";
    protected override void WriteCompatible(FamilyPacket packet) => _sink.Write(packet.Record, packet.Text);
    public override void Dispose() => _sink.Dispose();
}
