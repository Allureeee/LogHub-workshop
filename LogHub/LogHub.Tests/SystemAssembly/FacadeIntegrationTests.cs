using LogHub.Building;
using LogHub.Configuration;
using LogHub.Processing;
using Hub = global::LogHub.LogHub;
namespace LogHub.Tests.SystemAssembly;
[TestClass]
public sealed class FacadeIntegrationTests
{
    [TestMethod]
    public void Facade_DefaultConfigurationRunsFullPipelineWithOnlyMemoryResources()
    {
        var resources = new MemoryResources();
        var result = Hub.Run(resources: resources);
        Assert.AreEqual(new ProcessingResult(40, 20), result);
        Assert.HasCount(40, resources.ConsoleSink.Texts); Assert.HasCount(40, resources.FileSink.Texts);
        Assert.AreEqual(1, resources.ConsoleSink.DisposeCount); Assert.AreEqual(1, resources.FileSink.DisposeCount);
    }
    [TestMethod]
    [DataRow("fluent")]
    [DataRow("classic")]
    public void Facade_SelectsEitherBuilderFromConfiguration(string builder)
    {
        var resources = new MemoryResources();
        Assert.AreEqual(new ProcessingResult(40, 20), Hub.Run(Samples.Profile(builder: builder), resources));
        Assert.HasCount(40, resources.FileSink.Texts);
    }
    [TestMethod]
    [DataRow("object")]
    [DataRow("class")]
    public void Production_IntegratesBothExternalApis_AndBothAdapterKinds(string adapter)
    {
        var resources = new MemoryResources(); var profile = Samples.Profile("production", adapter: adapter);
        profile.Sink = "console,file,document";
        Assert.AreEqual(new ProcessingResult(40, 20), Hub.Run(profile, resources));
        Assert.HasCount(40, resources.ConsoleSink.Texts); Assert.HasCount(40, resources.FileSink.Texts);
        Assert.HasCount(1, resources.Sessions); Assert.HasCount(40, resources.Sessions[0].Rows);
        Assert.HasCount(1, resources.Channels); Assert.HasCount(40, resources.Channels[0].Documents);
        Assert.AreEqual(1, resources.Sessions[0].OpenCount); Assert.AreEqual(1, resources.Sessions[0].DisposeCount);
        Assert.AreEqual(1, resources.Channels[0].DisposeCount);
    }
    [TestMethod]
    public void NoAcceptedRecords_DoNotCreateFileDatabaseOrDocumentConnections()
    {
        var resources = new MemoryResources { Input = "bad input\n" }; var profile = Samples.Profile("production");
        profile.Sink = "console,file,document";
        Assert.AreEqual(new ProcessingResult(0, 1), Hub.Run(profile, resources));
        Assert.AreEqual(0, resources.FileCreations); Assert.IsEmpty(resources.Sessions); Assert.IsEmpty(resources.Channels);
    }
    [TestMethod]
    public void FinalPipeline_PushAndPullRemainEquivalent()
    {
        var push = new MemoryResources(); var pull = new MemoryResources(); var config = Samples.Profile();
        var first = Hub.Run(config, push); config.Notifications = "pull";
        var second = Hub.Run(config, pull);
        Assert.AreEqual(first, second); CollectionAssert.AreEqual(push.ConsoleSink.Texts, pull.ConsoleSink.Texts);
    }
    [TestMethod]
    public void CompositeRuleParticipatesInExistingFilterChain()
    {
        var resources = new MemoryResources(); var profile = Samples.Profile();
        profile.RuleJson = "{\"op\":\"and\",\"children\":[{\"op\":\"severity\",\"value\":\"Warning\"},{\"op\":\"not\",\"child\":{\"op\":\"message\",\"value\":\"healthcheck\"}}]}";
        Assert.AreEqual(new ProcessingResult(10, 50), Hub.Run(profile, resources));
        Assert.HasCount(10, resources.ConsoleSink.Texts);
    }
    [TestMethod]
    public void FailingReceiverDoesNotBlockOtherReceivers()
    {
        var resources = new MemoryResources(); resources.FileSink.FailAlways = true;
        var profile = Samples.Profile("production"); profile.Delivery.Decorators.Clear();
        var result = Hub.Run(profile, resources);
        Assert.AreEqual(40, result.Written); Assert.AreEqual(40, result.NotificationErrors);
        Assert.HasCount(40, resources.ConsoleSink.Texts); Assert.HasCount(40, resources.Sessions[0].Rows);
    }
    [TestMethod]
    public void FailureDuringFinalPartialFlushIsIncludedInResult()
    {
        var resources = new MemoryResources(); resources.FileSink.FailOnAttempt = 36;
        var profile = Samples.Profile(); profile.Delivery.Decorators = new() { "buffer" }; profile.Delivery.BatchSize = 7;
        var result = Hub.Run(profile, resources);
        Assert.AreEqual(40, result.Written); Assert.AreEqual(1, result.NotificationErrors);
        Assert.HasCount(40, resources.ConsoleSink.Texts);
    }
    [TestMethod]
    public void DisposalFailureIsNotReportedAsSuccessfulCompletion()
    {
        var resources = new MemoryResources(); resources.FileSink.ThrowOnDispose = true;
        Assert.ThrowsExactly<AggregateException>(() => Hub.Run(Samples.Profile(), resources));
        Assert.AreEqual(1, resources.ConsoleSink.DisposeCount);
    }
    [TestMethod]
    public void LegacyProfileStillUsesOldFormatAndSameCounters()
    {
        var resources = new MemoryResources(); var profile = Samples.Profile("legacy"); profile.Delivery.Decorators.Clear();
        Assert.AreEqual(new ProcessingResult(40, 20), Hub.Run(profile, resources));
        Assert.AreEqual("2026-10-07 11:30:00 [Info] simple-pipe-0", resources.ConsoleSink.Texts[0]);
    }
    [TestMethod]
    public void FinalVariant20ProfileRejectsMissingRequiredReceiver()
    {
        var profile = Samples.Profile(); profile.Sink = "console";
        Assert.ThrowsExactly<ArgumentException>(() => Hub.Run(profile, new MemoryResources()));
    }
    [TestMethod]
    public void RepeatedRunOfBuiltPipelineResetsLR4StateAndFlushesEachRun()
    {
        var resources = new MemoryResources(); var config = Samples.Profile();
        using var pipeline = new PipelineDirector(new StandardPipelineBuilder(resources)).Build(config);
        Assert.AreEqual(new ProcessingResult(40, 20), pipeline.Run());
        Assert.AreEqual(new ProcessingResult(40, 20), pipeline.Run());
        Assert.HasCount(80, resources.FileSink.Texts);
    }
}
