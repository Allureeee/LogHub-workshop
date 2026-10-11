namespace LogHub.Graphs;
public sealed record Node
{
    public string Name { get; }
    public Node(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
    public override string ToString() => Name;
}
