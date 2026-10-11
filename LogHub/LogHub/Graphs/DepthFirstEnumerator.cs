using System.Collections;
namespace LogHub.Graphs;
internal sealed class DepthFirstEnumerator : IEnumerator<Node>
{
    private readonly DependencyGraph _graph;
    private readonly Node? _root;
    private readonly Stack<Node> _pending = new();
    private readonly HashSet<Node> _visited = new();
    private Node? _current;
    private bool _disposed;
    public DepthFirstEnumerator(DependencyGraph graph, Node? root)
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
            var node = _pending.Pop();
            if (!_visited.Add(node)) continue;
            var children = _graph.DependenciesOf(node);
            for (var i = children.Count - 1; i >= 0; i--)
                if (!_visited.Contains(children[i])) _pending.Push(children[i]);
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
            _pending.Push(_root);
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
