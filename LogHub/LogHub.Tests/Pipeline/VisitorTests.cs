using System.Text.Json;
using LogHub.Formatting;
using LogHub.Models;
using LogHub.Visiting;
namespace LogHub.Tests.Pipeline;

[TestClass]
public sealed class VisitorTests
{
    private static LogRecord[] Entries() => new LogRecord[]
    {
        TestPipeline.Entry("normal"),
        new ExceptionLogEntry { Severity = "Error", Message = "failed", StackTrace = "at A()\nat B()" },
        new MetricLogEntry { Severity = "Info", Message = "cpu", Value = 12.5 }
    };
    [TestMethod]
    public void TextVisitor_DispatchesAllThreeOverloads()
    {
        var visitor = new LogFormatter("plain");
        var texts = Entries().Select(visitor.Format).ToArray();
        StringAssert.Contains(texts[0], "normal");
        Assert.IsFalse(texts[0].Contains("stackTrace=", StringComparison.Ordinal));
        StringAssert.Contains(texts[1], "stackTrace=at A()\\nat B()");
        StringAssert.Contains(texts[2], "value=12.5");
    }
    [TestMethod]
    public void StatisticsVisitor_DispatchesAllThreeOverloads()
    {
        var visitor = new StatisticsVisitor();
        foreach (LogRecord entry in Entries()) entry.Accept(visitor);
        Assert.AreEqual(1, visitor.SimpleCount); Assert.AreEqual(1, visitor.ExceptionCount);
        Assert.AreEqual(1, visitor.MetricCount); Assert.AreEqual(3, visitor.Total);
    }
    [TestMethod]
    public void KeyVisitor_DispatchesAllThreeOverloads()
    {
        var visitor = new DuplicateKeyVisitor();
        var kinds = new List<string>();
        foreach (LogRecord entry in Entries()) { entry.Accept(visitor); kinds.Add(visitor.Key.Kind); }
        CollectionAssert.AreEqual(new[] { "simple", "exception", "metric" }, kinds);
    }
    [TestMethod]
    public void NewVisitor_RequiresNoChangesToRecordClasses()
    {
        // A new visitor declared only in the TEST assembly; production entries are unchanged.
        var visitor = new TypeNameVisitor();
        foreach (LogRecord entry in Entries()) entry.Accept(visitor);
        CollectionAssert.AreEqual(new[] { "S", "E", "M" }, visitor.Kinds);
    }
    [TestMethod]
    public void EachConcreteClass_DeclaresItsOwnAccept()
    {
        foreach (var type in new[] { typeof(SimpleLogEntry), typeof(ExceptionLogEntry), typeof(MetricLogEntry) })
            Assert.AreEqual(type, type.GetMethod(nameof(LogRecord.Accept))!.DeclaringType);
        Assert.IsTrue(typeof(LogRecord).GetMethod(nameof(LogRecord.Accept))!.IsAbstract);
    }
    [TestMethod]
    public void JsonFormatting_EscapesStrings_AndKeepsExceptionDetails()
    {
        var entry = new ExceptionLogEntry { Severity = "Critical", Message = "quoted \"line\"\nnext", StackTrace = "C:\\path\nframe" };
        using var json = JsonDocument.Parse(new LogFormatter("json").Format(entry));
        Assert.AreEqual(entry.Message, json.RootElement.GetProperty("message").GetString());
        Assert.AreEqual(entry.StackTrace, json.RootElement.GetProperty("stackTrace").GetString());
        Assert.AreEqual("exception", json.RootElement.GetProperty("kind").GetString());
    }
    [TestMethod]
    public void JsonFormatting_KeepsMetricValueNumeric()
    {
        using var json = JsonDocument.Parse(new LogFormatter("json").Format(Entries()[2]));
        Assert.AreEqual(12.5, json.RootElement.GetProperty("value").GetDouble());
    }
    [TestMethod]
    public void StatisticsReset_ClearsAllCounters()
    {
        var visitor = new StatisticsVisitor();
        foreach (var entry in Entries()) entry.Accept(visitor);
        visitor.Reset(); Assert.AreEqual(0, visitor.Total);
    }
    private sealed class TypeNameVisitor : ILogEntryVisitor
    {
        public List<string> Kinds { get; } = new();
        public void Visit(SimpleLogEntry entry) => Kinds.Add("S");
        public void Visit(ExceptionLogEntry entry) => Kinds.Add("E");
        public void Visit(MetricLogEntry entry) => Kinds.Add("M");
    }
}
