using LogHub.Sinks;
namespace LogHub.Families;
public sealed class ConsoleFamilyOutput : FamilyOutput
{
    private readonly ILogSink _sink;
    public ConsoleFamilyOutput(ILogSink sink) => _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    public override FamilyKind Family => FamilyKind.Development;
    public override string Name => "console";
    protected override void WriteCompatible(FamilyPacket packet) => _sink.Write(packet.Record, packet.Text);
    public override void Dispose() => _sink.Dispose();
}
