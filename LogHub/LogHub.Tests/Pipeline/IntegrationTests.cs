using System.Reflection;
using LogHub.Application;
using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Models;
using LogHub.Observing;
using LogHub.Parsing;
using LogHub.Processing;
using Microsoft.Extensions.DependencyInjection;
namespace LogHub.Tests.Pipeline;

[TestClass]
public sealed class IntegrationTests
{
    [TestMethod]
    [DataRow("push")]
    [DataRow("pull")]
    public void Variant20_AllPatternsWorkTogether_WithExpectedCounts(string mode)
    {
        using var rig = new PipelineRig(Variant20PipelineFixture.Text, new LogConfiguration { Notifications = mode });
        Assert.AreEqual(new ProcessingResult(40, 20), rig.Processor.Run());
        Assert.AreEqual(20, rig.Statistics.SimpleCount); Assert.AreEqual(10, rig.Statistics.ExceptionCount);
        Assert.AreEqual(10, rig.Statistics.MetricCount); Assert.AreEqual(40, rig.Counter.Count);
        Assert.AreEqual(4, rig.Alerts.AlertCount); Assert.HasCount(40, rig.Sink.Texts);
        Assert.AreEqual(15, rig.Mediator.PauseCount); Assert.AreEqual(15, rig.Mediator.ResumeCount);
        Assert.AreEqual(4, rig.Pipeline.MaximumBuffered);
    }
    [TestMethod]
    public void Variant20_PushAndPull_HaveIdenticalOutput()
    {
        using var push = new PipelineRig(Variant20PipelineFixture.Text, new LogConfiguration { Notifications = "push" });
        using var pull = new PipelineRig(Variant20PipelineFixture.Text, new LogConfiguration { Notifications = "pull" });
        Assert.AreEqual(push.Processor.Run(), pull.Processor.Run());
        CollectionAssert.AreEqual(push.Sink.Texts, pull.Sink.Texts);
        Assert.AreEqual(push.AlertText.ToString(), pull.AlertText.ToString());
    }
    [TestMethod]
    public void EmptyChain_RemovesOnlyTheFiveInvalidInputLines()
    {
        using var rig = new PipelineRig(Variant20PipelineFixture.Text, new LogConfiguration { Filters = "" });
        Assert.AreEqual(new ProcessingResult(55, 5), rig.Processor.Run());
    }
    [TestMethod]
    public void TwoReceivers_GetSameRecords_WithoutDoublingProcessedCount()
    {
        using var other = new MemoryLogSink();
        using var rig = new PipelineRig(Variant20PipelineFixture.Text,
            additionalObservers: new[] { new SinkObserver<MemoryLogSink>(other) });
        Assert.AreEqual(40, rig.Processor.Run().Written);
        CollectionAssert.AreEqual(rig.Sink.Texts, other.Texts);
    }
    [TestMethod]
    public void BrokenReceiver_DoesNotStopCounterAlertsOrHealthyReceiver()
    {
        using var broken = new MemoryLogSink { Fail = true };
        using var rig = new PipelineRig(Variant20PipelineFixture.Text,
            additionalObservers: new[] { new SinkObserver<MemoryLogSink>(broken) });
        var result = rig.Processor.Run();
        Assert.AreEqual(40, result.Written); Assert.AreEqual(40, result.NotificationErrors);
        Assert.AreEqual(40, rig.Counter.Count); Assert.HasCount(40, rig.Sink.Records);
    }
    [TestMethod]
    public void RepeatedRun_ResetsStatefulFiltersStatisticsAndObservers()
    {
        using var rig = new PipelineRig(Variant20PipelineFixture.Text);
        var first = rig.Processor.Run(); var second = rig.Processor.Run();
        Assert.AreEqual(first, second); Assert.AreEqual(40, rig.Counter.Count);
        Assert.AreEqual(40, rig.Statistics.Total); Assert.HasCount(80, rig.Sink.Records);
    }
    [TestMethod]
    public void StrategyReplacement_StillWorksThroughMediator()
    {
        var text = "2026-10-07T11:30:00|Info|pipe\n" +
            "{\"date\":\"2026-10-07T11:30:00\",\"level\":\"Info\",\"message\":\"json\"}";
        using var rig = new PipelineRig(text);
        Assert.AreEqual(new ProcessingResult(2, 0), rig.Processor.Run());
        rig.Processor.ChangeImporter(new JsonLogImporter(new InMemoryTextSource(text), new LogParser()));
        Assert.AreEqual(new ProcessingResult(1, 1), rig.Processor.Run());
    }
    [TestMethod]
    public void Variant20_ParsesBothFormatsAndAllThreeEntryKinds()
    {
        var importer = new MixedLogImporter(new InMemoryTextSource(Variant20PipelineFixture.Text), new LogParser());
        var items = importer.Import().ToArray();
        Assert.HasCount(60, items); Assert.AreEqual(55, items.Count(x => x.Record is not null));
        Assert.IsInstanceOfType<SimpleLogEntry>(items[0].Record);
        Assert.IsInstanceOfType<SimpleLogEntry>(items[1].Record);
        Assert.IsInstanceOfType<ExceptionLogEntry>(items[2].Record);
        Assert.IsInstanceOfType<MetricLogEntry>(items[3].Record);
    }
    [TestMethod]
    public void InvalidTypedJson_IsRejected_WithoutBreakingNextRecord()
    {
        var parser = new LogParser();
        Assert.IsNull(parser.Parse("{\"date\":\"2026-10-07T11:30:00Z\",\"level\":\"Info\",\"message\":\"m\",\"kind\":\"metric\",\"value\":\"bad\"}"));
        Assert.IsNull(parser.Parse("{\"date\":\"2026-10-07T11:30:00Z\",\"level\":\"Info\",\"message\":\"m\",\"kind\":\"other\"}"));
        Assert.IsNotNull(parser.Parse("2026-10-07T11:30:00|Info|valid"));
    }
    [TestMethod]
    public void Configuration_ParsesAllPipelineOptions_WithoutDisk()
    {
        var config = new ConfigReader().Parse(new[] { "filters=duplicates,age", "notifications=pull",
            "maxAgeMinutes=30", "bufferCapacity=2", "referenceTimeUtc=2026-10-07T12:00:00Z" }, Path.GetFullPath("."));
        Assert.AreEqual("duplicates,age", config.Filters); Assert.AreEqual("pull", config.Notifications);
        Assert.AreEqual(30, config.MaxAgeMinutes); Assert.AreEqual(2, config.BufferCapacity);
        Assert.AreEqual(TestPipeline.Now, config.ReferenceTimeUtc!.Value);
    }
    [TestMethod]
    [DataRow("bufferCapacity=0")]
    [DataRow("maxAgeMinutes=-1")]
    [DataRow("notifications=invalid")]
    [DataRow("referenceTimeUtc=nonsense")]
    public void InvalidConfiguration_ProducesAnExplicitError(string line) =>
        TestPipeline.Throws<ArgumentException>(() => new ConfigReader().Parse(new[] { line }, Path.GetFullPath(".")));
    [TestMethod]
    public void CompositionRoot_ResolvesApplicationAndAllFilters_WithoutExternalResources()
    {
        var services = CompositionRoot.CreateServices(new LogConfiguration { Sink = "console", Importer = "memory" });
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
            { ValidateScopes = true, ValidateOnBuild = true });
        Assert.IsNotNull(provider.GetRequiredService<LogHubApplication>());
        var names = provider.GetServices<LogFilter>().Select(f => f.Name).ToArray();
        Assert.IsTrue(new[] { "severity", "age", "duplicates" }.All(name => names.Contains(name)));
    }
}
