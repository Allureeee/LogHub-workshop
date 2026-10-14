using LogHub.Models;
namespace LogHub.Families;
public enum FamilyKind { Development, Production }
public sealed record FamilyPacket(FamilyKind Family, LogRecord Record, string Text);
public abstract class FamilyFormatter
{
    public abstract FamilyKind Family { get; }
    public abstract FamilyPacket Format(LogRecord record);
}
public abstract class FamilyEndpoint : IDisposable
{
    public abstract FamilyKind Family { get; }
    public abstract string Name { get; }
    public void Write(FamilyPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        if (packet.Family != Family)
            throw new ArgumentException($"{Name}: expected {Family} payload, got {packet.Family}.");
        WriteCompatible(packet);
    }
    protected abstract void WriteCompatible(FamilyPacket packet);
    public abstract void Dispose();
}
public abstract class FamilyOutput : FamilyEndpoint { }
public abstract class FamilyStorage : FamilyEndpoint { }
