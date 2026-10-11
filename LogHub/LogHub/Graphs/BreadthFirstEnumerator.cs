using System.Collections;
namespace LogHub.Graphs;

internal sealed class BreadthFirstEnumerator : IEnumerator<Node>
{
    private readonly DependencyGraph _graph;
    private readonly Node? _root;
    private readonly Queue<Node> _pending = new();
    private readonly HashSet<Node> _visited = new();
    private Node? _current;
    private bool _disposed;
    public BreadthFirstEnumerator(DependencyGraph graph, Node? root)
    {
        _graph = graph;
        _root = root;
        Reset();
    }
    public Node Current => _current ?? throw new InvalidOperationException(
        "Current is only valid after a successful MoveNext().");
    object IEnumerator.Current => Current;
    public bool MoveNext()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _current = null;
        while (_pending.Count > 0)
        {
            var node = _pending.Dequeue();
            foreach (var child in _graph.DependenciesOf(node))
                if (_visited.Add(child)) _pending.Enqueue(child);
            _current = node;
            return true;
        }
        return false;
    }
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _current = null;
        _pending.Clear();
        _visited.Clear();
        if (_root is not null)
        {
            _pending.Enqueue(_root);
            _visited.Add(_root);
        }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _pending.Clear();
        _visited.Clear();
        _current = null;
        _disposed = true;
    }
}
