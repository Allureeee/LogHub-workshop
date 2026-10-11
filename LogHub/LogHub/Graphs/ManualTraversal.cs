using System.Collections;
namespace LogHub.Graphs;

internal sealed class ManualTraversal : IEnumerable<Node>
{
    private readonly DependencyGraph _graph;
    private readonly Node? _root;
    private readonly bool _depthFirst;
    public ManualTraversal(DependencyGraph graph, Node? root, bool depthFirst)
    {
        _graph = graph;
        _root = root;
        _depthFirst = depthFirst;
    }
    public IEnumerator<Node> GetEnumerator() => _depthFirst
        ? new DepthFirstEnumerator(_graph, _root)
        : new BreadthFirstEnumerator(_graph, _root);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
