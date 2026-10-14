using LogHub.Building;
using LogHub.Configuration;
using LogHub.Importing.Sources;
using LogHub.Processing;
namespace LogHub.Tests.SystemAssembly;
[TestClass]
public sealed class BuilderAndPrototypeTests
{
    [TestMethod]
    public void FluentBuilder_ReportsMissingSource()
    {
        using var sink = new RecordingSink();
        var builder = new PipelineBuilder(new MemoryResources()).WriteTo(sink);
        var error = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Missing source: call FromFile, FromSource or From.", error.Message);
    }
    [TestMethod]
    public void FluentBuilder_ReportsMissingReceiver()
    {
        var builder = new PipelineBuilder(new MemoryResources()).FromSource(new InMemoryTextSource(Samples.TwoFormats));
        var error = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Missing output: call WriteTo or UseConfiguredOutputs.", error.Message);
    }
    [TestMethod]
    public void ClassicBuilder_ReportsMissingProcessingStep()
    {
        var builder = new StandardPipelineBuilder(new MemoryResources());
        builder.Reset(Samples.Profile()); builder.BuildSource(); builder.BuildOutputs();
        Assert.ThrowsExactly<InvalidOperationException>(() => builder.GetResult());
    }
    [TestMethod]
    public void BothBuilders_ProduceEquivalentRealPipelines()
    {
        var first = new MemoryResources(); var second = new MemoryResources();
        var config = Samples.Profile();
        using var fluent = new PipelineBuilder(first).Configure(config).FromFile(config.LogPath).UseConfiguredOutputs().Build();
        using var classic = new PipelineDirector(new StandardPipelineBuilder(second)).Build(config);
        Assert.AreEqual(new ProcessingResult(40, 20), fluent.Run());
        Assert.AreEqual(new ProcessingResult(40, 20), classic.Run());
        CollectionAssert.AreEqual(first.ConsoleSink.Texts, second.ConsoleSink.Texts);
        CollectionAssert.AreEqual(first.FileSink.Texts, second.FileSink.Texts);
    }
    [TestMethod]
    public void Builder_SnapshotsConfigurationBeforeCallerChangesIt()
    {
        var config = Samples.Profile(); var resources = new MemoryResources();
        var builder = new PipelineBuilder(resources).Configure(config).FromFile(config.LogPath).UseConfiguredOutputs();
        config.MinSeverity = "Critical"; config.Delivery.Retry.Attempts = 0;
        using var pipeline = builder.Build();
        Assert.AreEqual(40, pipeline.Run().Written);
    }
    [TestMethod]
    public void SuccessfulBuilder_CannotTransferOwnedSinksTwice()
    {
        var sink = new RecordingSink();
        var builder = new PipelineBuilder(new MemoryResources()).Configure(new LogConfiguration { Filters = "" })
            .FromSource(new InMemoryTextSource(Samples.TwoFormats)).WriteTo(sink);
        using (var pipeline = builder.Build()) Assert.AreEqual(2, pipeline.Run().Written);
        Assert.AreEqual(1, sink.DisposeCount);
        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }
    [TestMethod]
    public void Builder_RejectsDuplicateReceiverOwnership()
    {
        using var sink = new RecordingSink();
        var builder = new PipelineBuilder(new MemoryResources()).WriteTo(sink);
        Assert.ThrowsExactly<ArgumentException>(() => builder.WriteTo(sink));
    }
    [TestMethod]
    public void ShallowClone_SharesNestedRetryAndDecoratorList()
    {
        var original = Samples.Profile(); var clone = original.ShallowClone();
        clone.Delivery.Retry.Attempts = 9; clone.Delivery.Decorators.Clear();
        Assert.AreSame(original.Delivery, clone.Delivery);
        Assert.AreEqual(9, original.Delivery.Retry.Attempts);
        Assert.IsEmpty(original.Delivery.Decorators);
    }
    [TestMethod]
    public void DeepClone_DoesNotShareNestedObjectsOrLists()
    {
        var original = Samples.Profile(); var clone = original.DeepClone();
        clone.Delivery.Retry.Attempts = 9; clone.Delivery.Decorators.Clear(); clone.Family = "production";
        Assert.AreNotSame(original.Delivery, clone.Delivery);
        Assert.AreNotSame(original.Delivery.Retry, clone.Delivery.Retry);
        Assert.AreEqual(3, original.Delivery.Retry.Attempts);
        Assert.HasCount(3, original.Delivery.Decorators);
        Assert.AreEqual("development", original.Family);
    }
    [TestMethod]
    public void ProfileCatalog_IsolatedFromRegistrationAndReturnedCloneMutations()
    {
        var catalog = new ProfileCatalog(); var original = Samples.Profile();
        catalog.Register("base", original); original.Delivery.Retry.Attempts = 100;
        var first = catalog.Create("base"); first.Delivery.Decorators.Clear();
        var second = catalog.Create("base");
        Assert.AreEqual(3, second.Delivery.Retry.Attempts); Assert.HasCount(3, second.Delivery.Decorators);
    }
    [TestMethod]
    public void UnknownPrototype_IsRejected() => Assert.ThrowsExactly<KeyNotFoundException>(() => new ProfileCatalog().Create("missing"));
    [TestMethod]
    public void ConfigReader_ParsesAllFinalOptionsWithoutFileAccess()
    {
        var config = new ConfigReader().Parse(new[] { "family=production", "builder=classic", "adapter=class",
            "decorators=retry,buffer,timing", "batchSize=7", "retryAttempts=4", "databasePath=../output/test.sqlite",
            "rule={\"op\":\"not\",\"child\":{\"op\":\"message\",\"value\":\"healthcheck\"}}" }, Path.GetFullPath("profiles"));
        Assert.AreEqual("production", config.Family); Assert.AreEqual("classic", config.Builder); Assert.AreEqual("class", config.Adapter);
        Assert.AreEqual(7, config.Delivery.BatchSize); Assert.AreEqual(4, config.Delivery.Retry.Attempts);
        CollectionAssert.AreEqual(new[] { "retry", "buffer", "timing" }, config.Delivery.Decorators);
    }
    [TestMethod]
    [DataRow("family=unknown")]
    [DataRow("builder=unknown")]
    [DataRow("adapter=unknown")]
    [DataRow("batchSize=0")]
    [DataRow("retryAttempts=0")]
    public void InvalidFinalConfiguration_IsRejected(string line) =>
        Assert.ThrowsExactly<ArgumentException>(() => new ConfigReader().Parse(new[] { line }, Path.GetFullPath(".")));
}
