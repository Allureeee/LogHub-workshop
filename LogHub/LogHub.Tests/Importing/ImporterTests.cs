using System.Reflection;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Models;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;
namespace LogHub.Tests.Importing;
[TestClass]
public sealed class ImporterTests
{
    private const string Pipe = "2026-10-07T10:00:00|Info|pipe";
    private const string Json = "{\"date\":\"2026-10-07T10:01:00\",\"level\":\"Error\",\"message\":\"json\"}";

    [TestMethod]
    public void Mixed_RecognizesEachRecord_AndPreservesOrder()
    {
        var importer = Mixed(Pipe + "\n" + Json + "\n" + Pipe);
        CollectionAssert.AreEqual(new[] { "pipe", "json", "pipe" },
            importer.Import().Select(x => x.Record!.Message).ToArray());
    }
    [TestMethod]
    public void Strategy_CanBeReplacedOnSameProcessor()
    {
        var text = Pipe + "\n" + Json;
        using var sink = new MemorySink();
        var processor = Processor(Mixed(text), sink);
        var first = processor.Run();
        processor.ChangeImporter(new JsonLogImporter(new InMemoryTextSource(text), new LogParser()));
        var second = processor.Run();
        Assert.AreEqual(new ProcessingResult(2, 0), first);
        Assert.AreEqual(new ProcessingResult(1, 1), second);
    }
    [TestMethod]
    public void Template_CallsStepsInOrder_VerifiedByRecordingMock()
    {
        var mock = new RecordingImporterMock(Pipe);
        _ = mock.Import().ToArray();
        CollectionAssert.AreEqual(new[] { "open", "read", "parse", "validate", "read", "close", "release" }, mock.Calls);
    }
    [TestMethod]
    public void Template_IsNonOverridable_ReadAndParseAreAbstract()
    {
        var type = typeof(LogImporterBase);
        var method = type.GetMethod(nameof(LogImporterBase.Import))!;
        // Implicit interface implementations may be virtual+final in CLR metadata.
        Assert.IsTrue(!method.IsVirtual || method.IsFinal);
        Assert.IsTrue(type.GetMethod("Read", BindingFlags.Instance | BindingFlags.NonPublic)!.IsAbstract);
        Assert.IsTrue(type.GetMethod("Parse", BindingFlags.Instance | BindingFlags.NonPublic)!.IsAbstract);
    }
    [TestMethod]
    public void DefaultHook_InMemoryDoesNotRejectAnEmptyMessage()
    {
        var importer = new InMemoryLogImporter(new InMemoryTextSource("2026-10-07T10:00:00|Info|"), new LogParser());
        Assert.IsNotNull(importer.Import().Single().Record);
    }
    [TestMethod]
    public void DefaultHook_JsonDoesNotRejectAnUnknownLevel()
    {
        var importer = new JsonLogImporter(new InMemoryTextSource(Json.Replace("Error", "Notice")), new LogParser());
        Assert.IsNotNull(importer.Import().Single().Record);
    }
    [TestMethod]
    public void OverriddenHook_RejectsEmptyMessage()
    {
        Assert.IsNull(Mixed("2026-10-07T10:00:00|Info|   ").Import().Single().Record);
    }
    [TestMethod]
    public void OverriddenHook_RejectsUnknownLevel()
    {
        Assert.IsNull(Mixed("2026-10-07T10:00:00|Unknown|message").Import().Single().Record);
    }
    [TestMethod]
    public void OverriddenHook_AcceptsKnownLevelCaseInsensitively()
    {
        using var sink = new MemorySink();
        var processor = TestPipeline.CreateProcessor(Mixed(Pipe.Replace("Info", "error")),
            new SeverityFilter("Error"), new LogFormatter("plain"), new[] { sink });
        Assert.AreEqual(1, processor.Run().Written);
    }
    [TestMethod]
    public void JsonImporter_RejectsPipeFormat()
    {
        var importer = new JsonLogImporter(new InMemoryTextSource(Pipe), new LogParser());
        Assert.IsNull(importer.Import().Single().Record);
    }
    [TestMethod]
    public void MalformedJson_IsRejected_AndNextRecordIsProcessed()
    {
        using var sink = new MemorySink();
        var result = Processor(Mixed("{not json\n" + Pipe), sink).Run();
        Assert.AreEqual(new ProcessingResult(1, 1), result);
    }
    [TestMethod]
    [DataRow("{\"date\":7,\"level\":\"Info\",\"message\":\"x\"}")]
    [DataRow("{\"date\":\"2026-10-07T10:00:00\",\"level\":7,\"message\":\"x\"}")]
    [DataRow("{\"date\":\"2026-10-07T10:00:00\",\"level\":\"Info\",\"message\":{}}")]
    [DataRow("{\"date\":\"2026-10-07T10:00:00\",\"level\":\"Info\",\"message\":null}")]
    [DataRow("[]")]
    [DataRow("42")]
    public void JsonParser_RejectsWrongShapesWithoutThrowing(string line)
    {
        Assert.IsNull(new LogParser().ParseJson(line));
    }
    [TestMethod]
    public void EmptySource_ReturnsNoItems()
    {
        Assert.AreEqual(0, Mixed(string.Empty).Import().Count());
    }
    [TestMethod]
    public void BlankLine_IsCountedAsRejected()
    {
        using var sink = new MemorySink();
        Assert.AreEqual(new ProcessingResult(1, 1), Processor(Mixed("\n" + Pipe), sink).Run());
    }
    [TestMethod]
    public void MessagesMayContainPipeCharacters()
    {
        Assert.AreEqual("a|b|c", Mixed("2026-10-07T10:00:00|Info|a|b|c").Import().Single().Record!.Message);
    }
    [TestMethod]
    public void LeadingWhitespaceDoesNotPreventJsonDetection()
    {
        Assert.AreEqual("json", Mixed(" \t" + Json).Import().Single().Record!.Message);
    }
    [TestMethod]
    public void LineNumbersIncludeRejectedLines()
    {
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, Mixed(Pipe + "\nbad\n" + Json).Import().Select(x => x.LineNumber).ToArray());
    }
    [TestMethod]
    public void Import_IsLazy_UntilFirstMoveNext()
    {
        var mock = new RecordingImporterMock(Pipe);
        var sequence = mock.Import();
        Assert.IsEmpty(mock.Calls);
        using var iterator = sequence.GetEnumerator();
        Assert.IsEmpty(mock.Calls);
        Assert.IsTrue(iterator.MoveNext());
        Assert.AreEqual("open", mock.Calls[0]);
    }
    [TestMethod]
    public void RepeatedImports_HaveFreshReaders()
    {
        var importer = Mixed(Pipe + "\n" + Json);
        CollectionAssert.AreEqual(importer.Import().Select(x => x.Record!.Message).ToArray(),
            importer.Import().Select(x => x.Record!.Message).ToArray());
    }
    [TestMethod]
    public void EarlyBreak_StillClosesAndReleasesResource()
    {
        var mock = new RecordingImporterMock(Pipe + "\n" + Json);
        foreach (var item in mock.Import()) { Assert.IsNotNull(item.Record); break; }
        CollectionAssert.AreEqual(new[] { "open", "read", "parse", "validate", "close", "release" }, mock.Calls);
    }
    [TestMethod]
    [DataRow("read")]
    [DataRow("parse")]
    [DataRow("validate")]
    public void StepFailure_StillClosesAndReleasesResource(string step)
    {
        var mock = new RecordingImporterMock(Pipe) { FailAt = step };
        ExpectException<IOException>(() => mock.Import().ToArray());
        CollectionAssert.AreEqual(new[] { "close", "release" }, mock.Calls.TakeLast(2).ToArray());
    }
    [TestMethod]
    public void CloseFailure_DoesNotPreventRelease()
    {
        var mock = new RecordingImporterMock(Pipe) { FailAt = "close" };
        ExpectException<IOException>(() => mock.Import().ToArray());
        Assert.AreEqual("release", mock.Calls[^1]);
    }
    [TestMethod]
    public void SinkFailure_IsReported_AndDisposesImportEnumeration()
    {
        var mock = new RecordingImporterMock(Pipe);
        using var sink = new MemorySink { Fail = true };
        // LR7 requires subscriber exceptions to be isolated, not rethrown.
        var result = Processor(mock, sink).Run();
        Assert.AreEqual(1, result.NotificationErrors);
        CollectionAssert.AreEqual(new[] { "close", "release" }, mock.Calls.TakeLast(2).ToArray());
    }
    [TestMethod]
    public void TwoSinks_GetSameRecords_WithoutDoublingCounter()
    {
        using var first = new MemorySink();
        using var second = new MemorySink();
        var result = Processor(Mixed(Pipe + "\n" + Json), first, second).Run();
        Assert.AreEqual(new ProcessingResult(2, 0), result);
        Assert.HasCount(2, first.Records);
        CollectionAssert.AreEqual(first.Texts, second.Texts);
    }
    [TestMethod]
    public void SixtyLineVariant20Fixture_HasExpectedCounters()
    {
        var importer = Mixed(Variant20Fixture.Text);
        var items = importer.Import().ToArray();
        Assert.HasCount(60, items);
        Assert.AreEqual(50, items.Count(x => x.Record is not null));
        using var sink = new MemorySink();
        Assert.AreEqual(new ProcessingResult(40, 20), Processor(importer, sink).Run());
    }
    [TestMethod]
    public void EmptySinkList_IsRejected()
    {
        ExpectException<ArgumentException>(() => Processor(Mixed(Pipe)));
    }
    [TestMethod]
    public void NullStrategyReplacement_IsRejected()
    {
        using var sink = new MemorySink();
        ExpectException<ArgumentNullException>(() => Processor(Mixed(Pipe), sink).ChangeImporter(null!));
    }

    private static MixedLogImporter Mixed(string text) => new(new InMemoryTextSource(text), new LogParser());
    private static LogProcessor Processor(ILogImporter importer, params ILogSink[] sinks) =>
        TestPipeline.CreateProcessor(importer, new SeverityFilter("Info"), new LogFormatter("plain"), sinks);
    private static void ExpectException<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        Assert.Fail($"Expected {typeof(T).Name}.");
    }
    private sealed class MemorySink : ILogSink
    {
        public string Name => "test-memory";
        public bool Fail { get; init; }
        public List<LogRecord> Records { get; } = new();
        public List<string> Texts { get; } = new();
        public void Write(LogRecord record, string text)
        {
            if (Fail) throw new IOException("Injected sink failure.");
            Records.Add(record); Texts.Add(text);
        }
        public void Dispose() { }
    }
    private sealed class RecordingImporterMock : LogImporterBase
    {
        public List<string> Calls { get; } = new();
        public string? FailAt { get; init; }
        public RecordingImporterMock(string text) : base(new InMemoryTextSource(text)) { }
        private void Record(string step)
        {
            Calls.Add(step);
            if (FailAt == step) throw new IOException("Injected " + step + " failure.");
        }
        protected override TextReader Open() { Record("open"); return base.Open(); }
        protected override string? Read(TextReader reader) { Record("read"); return reader.ReadLine(); }
        protected override LogRecord? Parse(string line)
        {
            Record("parse");
            return new SimpleLogEntry { Date = new DateTime(2026, 10, 7), Severity = "Info", Message = line };
        }
        protected override bool Validate(LogRecord record) { Record("validate"); return true; }
        protected override void Close(TextReader reader) { Record("close"); base.Close(reader); }
        protected override void Release(TextReader reader) { Record("release"); base.Release(reader); }
    }
}
