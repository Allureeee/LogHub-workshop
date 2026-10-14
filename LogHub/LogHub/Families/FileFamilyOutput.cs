using LogHub.Sinks;
namespace LogHub.Families;
public sealed class FileFamilyOutput : FamilyOutput
{
    private readonly ILogSink _sink;
    public FileFamilyOutput(ILogSink sink) => _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    public override FamilyKind Family => FamilyKind.Production;
    public override string Name => "file";
    protected override void WriteCompatible(FamilyPacket packet) => _sink.Write(packet.Record, packet.Text);
    public override void Dispose() => _sink.Dispose();
}
