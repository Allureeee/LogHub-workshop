namespace LogHub.Families;
public sealed class MemoryFamilyStorage : FamilyStorage
{
    private readonly List<FamilyPacket> _packets = new();
    public override FamilyKind Family => FamilyKind.Development;
    public override string Name => "memory";
    public IReadOnlyList<FamilyPacket> Packets => _packets.AsReadOnly();
    protected override void WriteCompatible(FamilyPacket packet) => _packets.Add(packet);
    public override void Dispose() { }
}
