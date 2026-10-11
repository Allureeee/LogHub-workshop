namespace LogHub.Graphs;
public sealed class GraphDemo
{
    private readonly DependencyGraph _services;
    private readonly Node _root;
    private readonly DependencyGraph _cyclicExample;
    public GraphDemo(DependencyGraph services, Node root, DependencyGraph cyclicExample)
    {
        _services = services;
        _root = root;
        _cyclicExample = cyclicExample;
    }
    public void Print(TextWriter output)
    {
        output.WriteLine("DFS / yield:  " + string.Join(" -> ", _services.DepthFirst(_root)));
        output.WriteLine("DFS / manual: " + string.Join(" -> ", _services.DepthFirstManual(_root)));
        output.WriteLine("BFS / yield:  " + string.Join(" -> ", _services.BreadthFirst(_root)));
        output.WriteLine("BFS / manual: " + string.Join(" -> ", _services.BreadthFirstManual(_root)));
        output.WriteLine("Nested traversals (include each starting service itself):");
        foreach (var service in _services.DepthFirst(_root))
            foreach (var dependency in _services.BreadthFirst(service))
                output.WriteLine($"{service} -> {dependency}");
        output.WriteLine("Real service graph cycle: " +
            (_services.FindCycle().Count == 0 ? "none" : string.Join(" -> ", _services.FindCycle())));
        output.WriteLine("Artificial cycle example (NOT a DI registration): " +
            string.Join(" -> ", _cyclicExample.FindCycle()));
        output.WriteLine("Cyclic DFS / yield:  " + string.Join(" -> ",
            _cyclicExample.DepthFirst(_cyclicExample.Nodes[0])));
        output.WriteLine("Cyclic DFS / manual: " + string.Join(" -> ",
            _cyclicExample.DepthFirstManual(_cyclicExample.Nodes[0])));
        output.WriteLine("Cyclic BFS / yield:  " + string.Join(" -> ",
            _cyclicExample.BreadthFirst(_cyclicExample.Nodes[0])));
        output.WriteLine("Cyclic BFS / manual: " + string.Join(" -> ",
            _cyclicExample.BreadthFirstManual(_cyclicExample.Nodes[0])));
    }
}
