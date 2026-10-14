using System.Text.Json;
using LogHub.Adapters;
using LogHub.Catalog;
using LogHub.External;
using LogHub.Formatting;
using LogHub.Models;
using LogHub.Proxy;
using LogHub.Sinks;
namespace LogHub.Tests.SystemAssembly;
[TestClass]
public sealed class AdapterAndProxyTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SqlAdapters_TranslateAllArguments(bool useClass)
    {
        var session = new RecordingSqlSession(); var catalog = new SeverityCatalog();
        using ILogSink sink = useClass ? new SqlClassAdapter(session, catalog) : new SqlObjectAdapter(new SqlLogApi(session), catalog);
        var entry = Samples.Record(level: "Error");
        sink.Write(entry, "serialized payload");
        Assert.HasCount(1, session.Rows);
        Assert.AreEqual(entry.Date, session.Rows[0].Date); Assert.AreEqual(3, session.Rows[0].Severity);
        Assert.AreEqual("serialized payload", session.Rows[0].Message);
    }
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void JsonAdapters_PreserveTypedFields_AndConsumerOwnsItsClone(bool useClass)
    {
        var channel = new RecordingDocumentChannel();
        using ILogSink sink = useClass ? new JsonClassAdapter(channel, new LogFormatter("json"))
            : new JsonObjectAdapter(new JsonLogApi(channel), new LogFormatter("json"));
        var entry = new MetricLogEntry { Date = Samples.Record().Date, Severity = "Info", Message = "cpu", Value = 12.5 };
        sink.Write(entry, "ignored rendering");
        Assert.HasCount(1, channel.Documents);
        Assert.AreEqual("metric", channel.Documents[0].GetProperty("kind").GetString());
        Assert.AreEqual(12.5, channel.Documents[0].GetProperty("value").GetDouble());
    }
    [TestMethod]
    public void VirtualProxy_DoesNotOpenDatabaseBeforeFirstWrite()
    {
        var session = new RecordingSqlSession();
        using var proxy = new VirtualLogSink("database", () => new SqlObjectAdapter(new SqlLogApi(session), new SeverityCatalog()));
        Assert.IsFalse(proxy.IsCreated); Assert.AreEqual(0, session.OpenCount);
        proxy.Flush(); Assert.AreEqual(0, session.OpenCount);
        proxy.Write(Samples.Record(), "first"); proxy.Write(Samples.Record(), "second");
        Assert.IsTrue(proxy.IsCreated); Assert.AreEqual(1, session.OpenCount); Assert.HasCount(2, session.Rows);
    }
    [TestMethod]
    public void DisposingUnusedProxy_DoesNotCreateOrOpenTarget()
    {
        var calls = 0;
        var proxy = new VirtualLogSink("unused", () => { calls++; return new RecordingSink(); });
        proxy.Dispose(); proxy.Dispose();
        Assert.AreEqual(0, calls); Assert.IsFalse(proxy.IsCreated);
    }
    [TestMethod]
    public void UsedProxy_DisposesTargetOnce_AndRejectsLaterWrites()
    {
        var target = new RecordingSink(); var proxy = new VirtualLogSink("target", () => target);
        proxy.Write(Samples.Record(), "x"); proxy.Dispose(); proxy.Dispose();
        Assert.AreEqual(1, target.DisposeCount);
        Assert.ThrowsExactly<ObjectDisposedException>(() => proxy.Write(Samples.Record(), "y"));
    }
    [TestMethod]
    public void Cache_ReadsSourceOncePerNormalizedKey()
    {
        var source = new CountingCatalog(); var cache = new CachingSeverityCatalog(source);
        Assert.AreEqual(3, cache.GetCode("Error")); Assert.AreEqual(3, cache.GetCode("error"));
        Assert.AreEqual(1, cache.GetCode("Info")); Assert.AreEqual(2, source.Calls);
    }
    [TestMethod]
    public void Cache_InvalidationReloadsOnlySelectedKey()
    {
        var source = new CountingCatalog(); var cache = new CachingSeverityCatalog(source);
        cache.GetCode("Error"); cache.GetCode("Info");
        Assert.IsTrue(cache.Invalidate("ERROR"));
        cache.GetCode("Error"); cache.GetCode("Info");
        Assert.AreEqual(3, source.Calls);
    }
    [TestMethod]
    public void Cache_ClearReloadsAllKeys()
    {
        var source = new CountingCatalog(); var cache = new CachingSeverityCatalog(source);
        cache.GetCode("Error"); cache.GetCode("Info"); cache.Clear();
        cache.GetCode("Error"); cache.GetCode("Info"); Assert.AreEqual(4, source.Calls);
    }
    [TestMethod]
    public async Task Cache_ConcurrentReadsLoadOneValueOnly()
    {
        var source = new CountingCatalog(); var cache = new CachingSeverityCatalog(source);
        var values = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => cache.GetCode("Error"))));
        Assert.HasCount(20, values); Assert.AreEqual(1, source.Calls);
        foreach (var value in values) Assert.AreEqual(3, value);
    }
}
