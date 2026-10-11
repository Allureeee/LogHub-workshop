using System.Collections;
namespace LogHub.Graphs;
public sealed class DependencyGraph : IEnumerable<Node>
{
    private readonly IReadOnlyList<Node> _nodes;
    private readonly Dictionary<Node, IReadOnlyList<Node>> _edges;
    public IReadOnlyList<Node> Nodes => _nodes;
    public DependencyGraph(IEnumerable<Node> nodes,
        IEnumerable<(Node Service, Node Dependency)> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);
        var nodeArray = nodes.Distinct().ToArray();
        var adjacency = nodeArray.ToDictionary(n => n, _ => new List<Node>());
        foreach (var (service, dependency) in edges)
        {
            if (!adjacency.TryGetValue(service, out var children) ||
                !adjacency.ContainsKey(dependency))
                throw new ArgumentException("Both edge endpoints must be graph nodes.", nameof(edges));
            if (!children.Contains(dependency)) children.Add(dependency);
        }
        _nodes = Array.AsReadOnly(nodeArray);
        _edges = adjacency.ToDictionary(pair => pair.Key,
            pair => (IReadOnlyList<Node>)pair.Value.AsReadOnly());
    }

    public IReadOnlyList<Node> DependenciesOf(Node node) => _edges[node];
    private void ValidateRoot(Node? root)
    {
        if (root is not null && !_edges.ContainsKey(root))
            throw new ArgumentException("The root is not in this graph.", nameof(root));
    }
    public IEnumerable<Node> DepthFirst(Node? root)
    {
        ValidateRoot(root);
        return DepthFirstCore(root);
    }
    private IEnumerable<Node> DepthFirstCore(Node? root)
    {
        if (root is null) yield break;
        var pending = new Stack<Node>();
        var visited = new HashSet<Node>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var node = pending.Pop();
            if (!visited.Add(node)) continue;
            // Reverse push preserves the order in which edges were specified.
            var children = DependenciesOf(node);
            for (var i = children.Count - 1; i >= 0; i--)
                if (!visited.Contains(children[i])) pending.Push(children[i]);
            yield return node;
        }
    }
    public IEnumerable<Node> BreadthFirst(Node? root)
    {
        ValidateRoot(root);
        return BreadthFirstCore(root);
    }
    private IEnumerable<Node> BreadthFirstCore(Node? root)
    {
        if (root is null) yield break;
        var pending = new Queue<Node>();
        var visited = new HashSet<Node> { root };
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            var node = pending.Dequeue();
            foreach (var child in DependenciesOf(node))
                if (visited.Add(child)) pending.Enqueue(child);
            yield return node;
        }
    }
    public IEnumerable<Node> DepthFirstManual(Node? root)
    {
        ValidateRoot(root);
        return new ManualTraversal(this, root, depthFirst: true);
    }
    public IEnumerable<Node> BreadthFirstManual(Node? root)
    {
        ValidateRoot(root);
        return new ManualTraversal(this, root, depthFirst: false);
    }
    public IReadOnlyList<Node> FindCycle()
    {
        var completed = new HashSet<Node>();
        var active = new Dictionary<Node, int>();
        var path = new List<Node>();
        var frames = new Stack<(Node Node, int NextChild)>();
        foreach (var start in _nodes)
        {
            if (completed.Contains(start)) continue;
            active.Add(start, path.Count);
            path.Add(start);
            frames.Push((start, 0));
            while (frames.Count > 0)
            {
                var (node, nextChild) = frames.Pop();
                var children = DependenciesOf(node);
                if (nextChild == children.Count)
                {
                    completed.Add(node);
                    active.Remove(node);
                    path.RemoveAt(path.Count - 1);
                    continue;
                }
                frames.Push((node, nextChild + 1));
                var child = children[nextChild];
                if (active.TryGetValue(child, out var cycleStart))
                    return Array.AsReadOnly(path.Skip(cycleStart).Append(child).ToArray());
                if (completed.Contains(child)) continue;
                active.Add(child, path.Count);
                path.Add(child);
                frames.Push((child, 0));
            }
        }
        return Array.Empty<Node>();
    }
    public IEnumerator<Node> GetEnumerator() => _nodes.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
