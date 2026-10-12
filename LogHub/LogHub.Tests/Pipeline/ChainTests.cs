using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Models;
using LogHub.Visiting;
namespace LogHub.Tests.Pipeline;

[TestClass]
public sealed class ChainTests
{
    private static FilterChain Chain(string order, int maxAge = 60, IEnumerable<LogFilter>? extra = null)
    {
        var config = new LogConfiguration { Filters = order, MinSeverity = "Warning", MaxAgeMinutes = maxAge };
        var filters = new List<LogFilter>
        {
            new DuplicateLogFilter(new DuplicateKeyVisitor()),
            new SeverityLogFilter(new SeverityFilter(config.MinSeverity)),
            new AgeLogFilter(config, new FixedTimeProvider(TestPipeline.Now))
        };
        if (extra is not null) filters.AddRange(extra);
        return new FilterChain(config, filters);
    }
    [TestMethod]
    public void Severity_RejectsBelowThreshold_AndAcceptsBoundary()
    {
        var chain = Chain("severity");
        Assert.IsFalse(chain.Handle(TestPipeline.Entry(severity: "Info")));
        Assert.IsTrue(chain.Handle(TestPipeline.Entry(severity: "Warning")));
    }
    [TestMethod]
    public void Age_RejectsOldRecords_AndAcceptsBoundary()
    {
        var chain = Chain("age");
        Assert.IsFalse(chain.Handle(TestPipeline.Entry(ageMinutes: 61)));
        Assert.IsTrue(chain.Handle(TestPipeline.Entry(ageMinutes: 60)));
    }
    [TestMethod]
    public void Duplicates_RejectRepeatedContent_RegardlessOfTimestamp()
    {
        var chain = Chain("duplicates");
        Assert.IsTrue(chain.Handle(TestPipeline.Entry(ageMinutes: 20)));
        Assert.IsFalse(chain.Handle(TestPipeline.Entry(ageMinutes: 10)));
        Assert.IsTrue(chain.Handle(TestPipeline.Entry("different")));
    }
    [TestMethod]
    public void EmptyChain_AcceptsEveryNonNullRecord_EvenOldDebugDuplicates()
    {
        var chain = Chain("");
        var entry = TestPipeline.Entry(severity: "Debug", ageMinutes: 10000);
        Assert.IsTrue(chain.Handle(entry)); Assert.IsTrue(chain.Handle(entry));
    }
    [TestMethod]
    public void ConfigurationOrder_ChangesActualAcceptedRecords()
    {
        var reader = new ConfigReader();
        var first = reader.Parse(new[] { "filters=age,duplicates" }, Path.GetFullPath("."));
        var second = reader.Parse(new[] { "filters=duplicates,age" }, Path.GetFullPath("."));
        var entries = new[] { TestPipeline.Entry(ageMinutes: 120), TestPipeline.Entry(ageMinutes: 5) };
        var ageFirst = Chain(first.Filters); var duplicatesFirst = Chain(second.Filters);
        Assert.AreEqual(1, entries.Count(ageFirst.Handle));
        Assert.AreEqual(0, entries.Count(duplicatesFirst.Handle));
    }
    [TestMethod]
    public void RejectingLink_DoesNotInvokeRemainingLinks()
    {
        var probe = new ProbeFilter();
        var chain = Chain("severity,probe", extra: new[] { probe });
        Assert.IsFalse(chain.Handle(TestPipeline.Entry(severity: "Debug")));
        Assert.AreEqual(0, probe.Calls);
    }
    [TestMethod]
    public void NewFilter_IsUsedWithoutChangingChainCode()
    {
        var chain = Chain("no-healthcheck", extra: new[] { new NoHealthcheckFilter() });
        Assert.IsFalse(chain.Handle(TestPipeline.Entry("healthcheck")));
        Assert.IsTrue(chain.Handle(TestPipeline.Entry("actual event")));
    }
    [TestMethod]
    public void Reset_ClearsDuplicateMemory()
    {
        var chain = Chain("duplicates");
        Assert.IsTrue(chain.Handle(TestPipeline.Entry()));
        Assert.IsFalse(chain.Handle(TestPipeline.Entry()));
        chain.Reset();
        Assert.IsTrue(chain.Handle(TestPipeline.Entry()));
    }
    [TestMethod]
    public void UnknownFilter_IsAConfigurationError() =>
        TestPipeline.Throws<NotSupportedException>(() => Chain("missing"));
    [TestMethod]
    public void RepeatedFilterName_IsRejected_InsteadOfCreatingALinkCycle() =>
        TestPipeline.Throws<ArgumentException>(() => Chain("age,AGE"));
    [TestMethod]
    public void DuplicateKey_DistinguishesMetricValuesAndRecordKinds()
    {
        var chain = Chain("duplicates");
        Assert.IsTrue(chain.Handle(new MetricLogEntry { Severity = "Info", Message = "cpu", Value = 1 }));
        Assert.IsTrue(chain.Handle(new MetricLogEntry { Severity = "Info", Message = "cpu", Value = 2 }));
        Assert.IsTrue(chain.Handle(new SimpleLogEntry { Severity = "Info", Message = "cpu" }));
        Assert.IsFalse(chain.Handle(new MetricLogEntry { Severity = "info", Message = "cpu", Value = 1 }));
    }
    private sealed class ProbeFilter : LogFilter
    {
        public int Calls { get; private set; }
        public override string Name => "probe";
        protected override bool Allows(LogRecord entry) { Calls++; return true; }
    }
    private sealed class NoHealthcheckFilter : LogFilter
    {
        public override string Name => "no-healthcheck";
        protected override bool Allows(LogRecord entry) => !entry.Message.Contains("healthcheck", StringComparison.Ordinal);
    }
}
