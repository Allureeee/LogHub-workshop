using System.Collections;
using LogHub.Graphs;
namespace LogHub.Tests.Graphs;

[TestClass]
public sealed class GraphTests
{
    private static readonly string[] Dfs = { "A", "B", "D", "E", "C", "F" };
    private static readonly string[] Bfs = { "A", "B", "C", "D", "E", "F" };

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void DepthFirst_HasReferenceOrder(bool manual)
    {
        var (g, root) = Tree();
        Names(Dfs, manual ? g.DepthFirstManual(root) : g.DepthFirst(root));
    }
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void BreadthFirst_HasReferenceOrder(bool manual)
    {
        var (g, root) = Tree();
        Names(Bfs, manual ? g.BreadthFirstManual(root) : g.BreadthFirst(root));
    }
    [TestMethod]
    public void BothIteratorImplementations_ProduceSameSequences()
    {
        var (g, root) = Tree();
        CollectionAssert.AreEqual(g.DepthFirst(root).ToArray(), g.DepthFirstManual(root).ToArray());
        CollectionAssert.AreEqual(g.BreadthFirst(root).ToArray(), g.BreadthFirstManual(root).ToArray());
    }
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void NestedDepthAndBreadthFirst_HaveExactPairs(bool outerManual, bool innerManual)
    {
        var (g, root) = Tree();
        var pairs = new List<string>();
        foreach (var service in outerManual ? g.DepthFirstManual(root) : g.DepthFirst(root))
            foreach (var dependency in innerManual ? g.BreadthFirstManual(service) : g.BreadthFirst(service))
                pairs.Add(service.Name + ":" + dependency.Name);
        CollectionAssert.AreEqual(new[] { "A:A", "A:B", "A:C", "A:D", "A:E", "A:F",
            "B:B", "B:D", "B:E", "D:D", "E:E", "C:C", "C:F", "F:F" }, pairs);
    }
    [TestMethod]
    [DataRow("dfs")]
    [DataRow("bfs")]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void SameEnumerable_SupportsTwoIndependentEnumerators(string kind)
    {
        var (g, root) = Tree();
        var sequence = Traversal(g, root, kind);
        using var first = sequence.GetEnumerator();
        using var second = sequence.GetEnumerator();
        Assert.AreNotSame(first, second);
        Assert.IsTrue(first.MoveNext()); Assert.AreEqual("A", first.Current.Name);
        Assert.IsTrue(first.MoveNext()); Assert.AreEqual("B", first.Current.Name);
        Assert.IsTrue(second.MoveNext()); Assert.AreEqual("A", second.Current.Name);
        Assert.IsTrue(first.MoveNext());
        Assert.AreEqual(kind.StartsWith("dfs", StringComparison.Ordinal) ? "D" : "C", first.Current.Name);
    }
    [TestMethod]
    [DataRow("dfs")]
    [DataRow("bfs")]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void NestedLoopsOnTheSameEnumerable_DoNotCorruptState(string kind)
    {
        var (g, root) = Tree();
        var sequence = Traversal(g, root, kind);
        var count = 0;
        foreach (var outer in sequence)
            foreach (var inner in sequence)
            {
                Assert.IsNotNull(outer); Assert.IsNotNull(inner); count++;
            }
        Assert.AreEqual(36, count);
    }
    [TestMethod]
    [DataRow("dfs")]
    [DataRow("bfs")]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void CyclicGraph_VisitsEachNodeOnce(string kind)
    {
        var a = new Node("A"); var b = new Node("B"); var c = new Node("C");
        var g = new DependencyGraph(new[] { a, b, c }, new[] { (a, b), (b, c), (c, a) });
        Names(new[] { "A", "B", "C" }, Traversal(g, a, kind).Take(10));
    }
    [TestMethod]
    public void CycleDetection_ReturnsClosedPath()
    {
        var a = new Node("A"); var b = new Node("B"); var c = new Node("C");
        var g = new DependencyGraph(new[] { a, b, c }, new[] { (a, b), (b, c), (c, a) });
        Names(new[] { "A", "B", "C", "A" }, g.FindCycle());
    }
    [TestMethod]
    public void SelfLoop_ReturnsNodeTwiceInCyclePath()
    {
        var a = new Node("A");
        var g = new DependencyGraph(new[] { a }, new[] { (a, a) });
        Names(new[] { "A", "A" }, g.FindCycle());
        Names(new[] { "A" }, g.DepthFirst(a));
    }
    [TestMethod]
    public void SharedDependencyInDiamond_IsNotACycle()
    {
        var a = new Node("A"); var b = new Node("B"); var c = new Node("C"); var d = new Node("D");
        var g = new DependencyGraph(new[] { a, b, c, d }, new[] { (a, b), (a, c), (b, d), (c, d) });
        Assert.IsEmpty(g.FindCycle());
        Names(new[] { "A", "B", "D", "C" }, g.DepthFirst(a));
        Names(new[] { "A", "B", "C", "D" }, g.BreadthFirst(a));
        Names(new[] { "A", "B", "D", "C" }, g.DepthFirstManual(a));
        Names(new[] { "A", "B", "C", "D" }, g.BreadthFirstManual(a));
    }
    [TestMethod]
    public void CycleInDisconnectedComponent_IsFound()
    {
        var a = new Node("A"); var b = new Node("B"); var c = new Node("C");
        var g = new DependencyGraph(new[] { a, b, c }, new[] { (b, c), (c, b) });
        Names(new[] { "A" }, g.DepthFirst(a));
        Names(new[] { "B", "C", "B" }, g.FindCycle());
    }
    [TestMethod]
    [DataRow("dfs")]
    [DataRow("bfs")]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void EmptyGraph_HasEmptyTraversal(string kind)
    {
        var g = new DependencyGraph(Array.Empty<Node>(), Array.Empty<(Node, Node)>());
        Assert.AreEqual(0, Traversal(g, null, kind).Count());
        Assert.IsEmpty(g.FindCycle());
    }
    [TestMethod]
    [DataRow("dfs")]
    [DataRow("bfs")]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void SingleNode_HasExactlyOneItem(string kind)
    {
        var a = new Node("A");
        var g = new DependencyGraph(new[] { a }, Array.Empty<(Node, Node)>());
        Names(new[] { "A" }, Traversal(g, a, kind));
    }
    [TestMethod]
    public void GraphIsEnumerable_ButIsNotItsOwnEnumerator()
    {
        var (g, _) = Tree();
        Assert.IsFalse(typeof(IEnumerator<Node>).IsAssignableFrom(g.GetType()));
        Assert.IsFalse(typeof(IEnumerator).IsAssignableFrom(g.GetType()));
        Assert.AreEqual(6, g.Count());
    }
    [TestMethod]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void ManualReset_RestartsTheSameEnumerator(string kind)
    {
        var (g, root) = Tree();
        using var iterator = Traversal(g, root, kind).GetEnumerator();
        Assert.IsTrue(iterator.MoveNext()); Assert.IsTrue(iterator.MoveNext());
        iterator.Reset();
        Names(kind == "dfs-manual" ? Dfs : Bfs, Drain(iterator));
        iterator.Reset();
        Assert.IsTrue(iterator.MoveNext()); Assert.AreEqual("A", iterator.Current.Name);
    }
    [TestMethod]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void ManualCurrent_IsInvalidBeforeAndAfterIteration(string kind)
    {
        var (g, root) = Tree();
        using var iterator = Traversal(g, root, kind).GetEnumerator();
        ExpectException<InvalidOperationException>(() => { _ = iterator.Current; });
        _ = Drain(iterator);
        ExpectException<InvalidOperationException>(() => { _ = iterator.Current; });
        Assert.IsFalse(iterator.MoveNext());
    }
    [TestMethod]
    [DataRow("dfs-manual")]
    [DataRow("bfs-manual")]
    public void ManualDispose_IsIdempotent_AndStopsFurtherUse(string kind)
    {
        var (g, root) = Tree();
        using var iterator = Traversal(g, root, kind).GetEnumerator();
        iterator.Dispose(); iterator.Dispose();
        ExpectException<ObjectDisposedException>(() => iterator.MoveNext());
        ExpectException<ObjectDisposedException>(() => iterator.Reset());
    }
    [TestMethod]
    public void GraphCopiesInput_AndDoesNotExposeMutableAdjacency()
    {
        var a = new Node("A"); var b = new Node("B");
        var nodes = new[] { a, b }; var edges = new[] { (a, b) };
        var g = new DependencyGraph(nodes, edges);
        nodes[0] = new Node("X"); edges[0] = (b, b);
        Names(new[] { "A", "B" }, g.DepthFirst(a));
        Assert.IsEmpty(g.FindCycle());
        Assert.IsFalse(g.DependenciesOf(a) is Node[]);
    }
    [TestMethod]
    public void UnknownRoot_IsRejected()
    {
        var (g, _) = Tree();
        ExpectException<ArgumentException>(() => g.DepthFirst(new Node("missing")));
        ExpectException<ArgumentException>(() => g.BreadthFirstManual(new Node("missing")));
    }
    [TestMethod]
    public void UnknownEdgeEndpoint_IsRejected()
    {
        var a = new Node("A"); var b = new Node("B");
        ExpectException<ArgumentException>(() => new DependencyGraph(new[] { a }, new[] { (a, b) }));
    }
    [TestMethod]
    public void DeepGraph_DoesNotRequireRecursiveCycleDetection()
    {
        var nodes = Enumerable.Range(0, 5000).Select(i => new Node(i.ToString())).ToArray();
        var edges = Enumerable.Range(0, nodes.Length - 1).Select(i => (nodes[i], nodes[i + 1]));
        var g = new DependencyGraph(nodes, edges);
        Assert.IsEmpty(g.FindCycle());
        Assert.AreEqual(nodes.Length, g.DepthFirst(nodes[0]).Count());
    }

    private static (DependencyGraph Graph, Node Root) Tree()
    {
        var n = "ABCDEF".Select(c => new Node(c.ToString())).ToArray();
        var g = new DependencyGraph(n, new[] { (n[0], n[1]), (n[0], n[2]),
            (n[1], n[3]), (n[1], n[4]), (n[2], n[5]) });
        return (g, n[0]);
    }
    private static IEnumerable<Node> Traversal(DependencyGraph g, Node? root, string kind) => kind switch
    {
        "dfs" => g.DepthFirst(root), "bfs" => g.BreadthFirst(root),
        "dfs-manual" => g.DepthFirstManual(root), "bfs-manual" => g.BreadthFirstManual(root),
        _ => throw new ArgumentException("Unknown test traversal.", nameof(kind))
    };
    private static Node[] Drain(IEnumerator<Node> iterator)
    {
        var result = new List<Node>();
        while (iterator.MoveNext()) result.Add(iterator.Current);
        return result.ToArray();
    }
    private static void Names(IEnumerable<string> expected, IEnumerable<Node> actual) =>
        CollectionAssert.AreEqual(expected.ToArray(), actual.Select(n => n.Name).ToArray());
    private static void ExpectException<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        Assert.Fail($"Expected {typeof(T).Name}.");
    }
}
