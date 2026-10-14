using LogHub.Catalog;
using LogHub.Models;
using LogHub.Rules;
namespace LogHub.Tests.SystemAssembly;
[TestClass]
public sealed class CompositeTests
{
    [TestMethod]
    public void SeverityLeaf_AcceptsBoundaryAndRejectsLowerLevel()
    {
        ILogRule rule = new SeverityAtLeast("Warning", new SeverityCatalog());
        Assert.IsTrue(rule.Matches(Samples.Record(level: "Warning"))); Assert.IsFalse(rule.Matches(Samples.Record()));
    }
    [TestMethod]
    public void MessageLeaf_IsCaseInsensitive()
    {
        ILogRule rule = new MessageContains("healthcheck");
        Assert.IsTrue(rule.Matches(Samples.Record("HEALTHCHECK ready")));
        Assert.IsFalse(rule.Matches(Samples.Record("real error")));
    }
    [TestMethod]
    public void AndOrNot_WorkThroughOneInterface_WithNestedRules()
    {
        ILogRule rule = new And(new SeverityAtLeast("Warning", new SeverityCatalog()),
            new Not(new MessageContains("healthcheck")));
        Assert.IsTrue(rule.Matches(Samples.Record("failure", "Error")));
        Assert.IsFalse(rule.Matches(Samples.Record("healthcheck", "Error")));
        Assert.IsFalse(rule.Matches(Samples.Record("failure", "Info")));
        ILogRule alternatives = new Or(rule, new MessageContains("urgent"));
        Assert.IsTrue(alternatives.Matches(Samples.Record("urgent", "Info")));
    }
    [TestMethod]
    public void EmptyAndAccepts_EmptyOrRejects()
    { Assert.IsTrue(new And().Matches(Samples.Record())); Assert.IsFalse(new Or().Matches(Samples.Record())); }
    [TestMethod]
    public void CompositesShortCircuitWithoutInvokingUnneededChildren()
    {
        var probe = new ProbeRule();
        Assert.IsFalse(new And(new Or(), probe).Matches(Samples.Record()));
        Assert.IsTrue(new Or(new And(), probe).Matches(Samples.Record()));
        Assert.AreEqual(0, probe.Calls);
    }
    [TestMethod]
    public void SameClientHandlesLeafAndComposite()
    {
        var leaf = new MessageContains("event"); ILogRule composite = new And(leaf, new Not(new MessageContains("healthcheck")));
        static bool Client(ILogRule rule) => rule.Matches(Samples.Record());
        Assert.AreEqual(Client(leaf), Client(composite));
    }
    [TestMethod]
    public void RuleParserBuildsNestedTreeFromConfiguration()
    {
        const string json = "{\"op\":\"and\",\"children\":[{\"op\":\"severity\",\"value\":\"Warning\"},{\"op\":\"not\",\"child\":{\"op\":\"message\",\"value\":\"healthcheck\"}}]}";
        var rule = new RuleParser(new SeverityCatalog()).Parse(json);
        Assert.IsTrue(rule.Matches(Samples.Record("failure", "Error")));
        Assert.IsFalse(rule.Matches(Samples.Record("healthcheck", "Error")));
    }
    [TestMethod]
    [DataRow("{\"op\":\"other\"}")]
    [DataRow("{\"op\":\"not\"}")]
    [DataRow("{\"op\":\"and\"}")]
    [DataRow("not-json")]
    public void InvalidRuleIsRejected(string json) =>
        Assert.ThrowsExactly<ArgumentException>(() => new RuleParser(new SeverityCatalog()).Parse(json));
    private sealed class ProbeRule : ILogRule
    {
        public int Calls { get; private set; }
        public bool Matches(LogRecord record) { Calls++; return true; }
    }
}
