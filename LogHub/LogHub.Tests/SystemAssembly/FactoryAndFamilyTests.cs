using System.Text.Json;
using LogHub.Catalog;
using LogHub.Creation;
using LogHub.Families;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Models;
using LogHub.Parsing;
using LogHub.Sinks;
namespace LogHub.Tests.SystemAssembly;
[TestClass]
public sealed class FactoryAndFamilyTests
{
    private static ImporterFactoryRegistry Registry() => new(new LogImporterFactory[]
        { new MixedImporterFactory(), new JsonImporterFactory(), new MemoryImporterFactory() });
    [TestMethod]
    [DataRow("mixed", typeof(MixedLogImporter), 2)]
    [DataRow("json", typeof(JsonLogImporter), 1)]
    [DataRow("memory", typeof(InMemoryLogImporter), 2)]
    public void Factory_ReturnsConfiguredImporter_AndPreservesItsParsing(string name, Type expected, int valid)
    {
        var importer = Registry().Get(name).Create(new InMemoryTextSource(Samples.TwoFormats), new LogParser());
        Assert.AreEqual(expected, importer.GetType());
        Assert.HasCount(valid, importer.Import().Where(item => item.Record is not null));
    }
    [TestMethod]
    public void UnknownImporter_IsRejected() => Assert.ThrowsExactly<NotSupportedException>(() => Registry().Get("unknown"));
    [TestMethod]
    public void FactoryRegistry_AcceptsAnExtensionWithoutChangingItsCode()
    {
        var registry = new ImporterFactoryRegistry(new LogImporterFactory[] { new ExtraFactory() });
        var importer = registry.Get("extra").Create(new InMemoryTextSource(Samples.TwoFormats), new LogParser());
        Assert.HasCount(2, importer.Import());
    }
    [TestMethod]
    public void DevelopmentFamily_CombinesConsoleMemoryAndDetailedFormat()
    {
        var resources = new MemoryResources();
        var family = new DevelopmentFactory(resources, Samples.Profile()).CreateFamily();
        using var output = new FamilyLogSink(family.Output, family.Formatter);
        using var storage = new FamilyLogSink(family.Storage, family.Formatter);
        var record = Samples.Record();
        output.Write(record, "unused"); storage.Write(record, "unused");
        Assert.AreEqual(FamilyKind.Development, family.Formatter.Family);
        Assert.IsInstanceOfType<MemoryFamilyStorage>(family.Storage);
        Assert.AreEqual("2026-10-07 11:30:00 [Info] event", resources.ConsoleSink.Texts[0]);
        Assert.HasCount(1, ((MemoryFamilyStorage)family.Storage).Packets);
        Assert.IsEmpty(resources.Sessions);
    }
    [TestMethod]
    public void ProductionFamily_IsLazy_AndStoresCompactPayloadWithoutLosingMetricValue()
    {
        var resources = new MemoryResources();
        var family = new ProductionFactory(resources, Samples.Profile("production"), new SeverityCatalog()).CreateFamily();
        using var output = new FamilyLogSink(family.Output, family.Formatter);
        using var storage = new FamilyLogSink(family.Storage, family.Formatter);
        Assert.AreEqual(0, resources.FileCreations); Assert.IsEmpty(resources.Sessions);
        var metric = new MetricLogEntry { Date = Samples.Record().Date, Severity = "Info", Message = "cpu", Value = 42.5 };
        output.Write(metric, "unused"); storage.Write(metric, "unused");
        Assert.AreEqual(FamilyKind.Production, family.Formatter.Family);
        Assert.AreEqual(1, resources.FileCreations); Assert.HasCount(1, resources.Sessions);
        Assert.AreEqual(1, resources.Sessions[0].OpenCount);
        using var json = JsonDocument.Parse(resources.Sessions[0].Rows[0].Message);
        Assert.AreEqual(42.5, json.RootElement.GetProperty("value").GetDouble());
        Assert.AreEqual(resources.FileSink.Texts[0], resources.Sessions[0].Rows[0].Message);
    }
    [TestMethod]
    [DataRow(false, false, true)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(true, false, true)]
    [DataRow(false, true, true)]
    public void MixedFamilies_AreRejectedBeforeOpeningResources(bool productionOutput, bool productionStorage, bool productionFormat)
    {
        var resources = new MemoryResources();
        var development = new DevelopmentFactory(resources, Samples.Profile());
        var production = new ProductionFactory(resources, Samples.Profile("production"), new SeverityCatalog());
        using var output = productionOutput ? production.CreateOutput() : development.CreateOutput();
        using var storage = productionStorage ? production.CreateStorage() : development.CreateStorage();
        var format = productionFormat ? production.CreateFormatter() : development.CreateFormatter();
        Assert.ThrowsExactly<ArgumentException>(() => new FamilyComponents(output, storage, format));
        Assert.AreEqual(0, resources.FileCreations); Assert.IsEmpty(resources.Sessions);
    }
    [TestMethod]
    public void EndpointRejectsForeignPayload_EvenWithoutAComponentsBundle()
    {
        var resources = new MemoryResources();
        using var output = new ProductionFactory(resources, Samples.Profile("production"), new SeverityCatalog()).CreateOutput();
        var foreign = new FamilyPacket(FamilyKind.Development, Samples.Record(), "text");
        Assert.ThrowsExactly<ArgumentException>(() => output.Write(foreign));
        Assert.AreEqual(0, resources.FileCreations);
    }
    [TestMethod]
    public void FamilySink_RejectsForeignFormatter()
    {
        var resources = new MemoryResources();
        using var output = new DevelopmentFactory(resources, Samples.Profile()).CreateOutput();
        var format = new ProductionFactory(resources, Samples.Profile("production"), new SeverityCatalog()).CreateFormatter();
        Assert.ThrowsExactly<ArgumentException>(() => new FamilyLogSink(output, format));
    }
    private sealed class ExtraFactory : LogImporterFactory
    {
        public override string Name => "extra";
        public override ILogImporter Create(ITextSource source, LogParser parser) => new InMemoryLogImporter(source, parser);
    }
}
