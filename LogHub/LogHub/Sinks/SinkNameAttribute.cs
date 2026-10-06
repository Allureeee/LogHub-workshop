namespace LogHub.Sinks;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class SinkNameAttribute : Attribute
{
    public SinkNameAttribute(string name)
    {
        Name = name;
    }
    public string Name { get; }
}