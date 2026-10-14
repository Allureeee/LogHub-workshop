using LogHub.Configuration;
using LogHub.Decorating;
using LogHub.Sinks;
namespace LogHub.Tests.SystemAssembly;
[TestClass]
public sealed class DecoratorTests
{
    [TestMethod]
    public void Retry_RepeatsTemporaryFailure_ThenWritesOnce()
    {
        var target = new RecordingSink { RemainingFailures = 1 };
        using var retry = new RetryLogSink(target, 3);
        retry.Write(Samples.Record(), "x");
        Assert.AreEqual(2, target.Attempts); Assert.HasCount(1, target.Records);
    }
    [TestMethod]
    public void Retry_StopsAtConfiguredLimit()
    {
        var target = new RecordingSink { FailAlways = true };
        using var retry = new RetryLogSink(target, 2);
        Assert.ThrowsExactly<IOException>(() => retry.Write(Samples.Record(), "x"));
        Assert.AreEqual(2, target.Attempts); Assert.IsEmpty(target.Records);
    }
    [TestMethod]
    public void Retry_DoesNotRepeatProgrammingErrors()
    {
        var target = new RecordingSink { ThrowArgument = true };
        using var retry = new RetryLogSink(target, 3);
        Assert.ThrowsExactly<ArgumentException>(() => retry.Write(Samples.Record(), "x"));
        Assert.AreEqual(1, target.Attempts);
    }
    [TestMethod]
    public void Timing_UsesInjectedClock_IncludingFailedCalls()
    {
        var clock = new ManualClock(); var reports = new List<WriteMeasurement>();
        var target = new RecordingSink { RemainingFailures = 1, OnWrite = () => clock.Advance(TimeSpan.FromMilliseconds(5)) };
        using var timing = new TimingLogSink(target, clock, reports.Add);
        Assert.ThrowsExactly<IOException>(() => timing.Write(Samples.Record(), "x"));
        timing.Write(Samples.Record(), "x");
        Assert.HasCount(2, reports); Assert.IsFalse(reports[0].Succeeded); Assert.IsTrue(reports[1].Succeeded);
        Assert.AreEqual(TimeSpan.FromMilliseconds(5), reports[0].Elapsed);
    }
    [TestMethod]
    public void DecoratorOrder_ChangesTimingObservationOfRetries()
    {
        var clock = new ManualClock(); var total = new List<WriteMeasurement>(); var attempts = new List<WriteMeasurement>();
        var first = new RecordingSink { RemainingFailures = 1, OnWrite = () => clock.Advance(TimeSpan.FromMilliseconds(5)) };
        var second = new RecordingSink { RemainingFailures = 1, OnWrite = () => clock.Advance(TimeSpan.FromMilliseconds(5)) };
        using var timingOutside = new TimingLogSink(new RetryLogSink(first, 2), clock, total.Add);
        using var retryOutside = new RetryLogSink(new TimingLogSink(second, clock, attempts.Add), 2);
        timingOutside.Write(Samples.Record(), "x"); retryOutside.Write(Samples.Record(), "x");
        Assert.HasCount(1, total); Assert.HasCount(2, attempts);
        Assert.AreEqual(TimeSpan.FromMilliseconds(10), total[0].Elapsed);
        Assert.AreEqual(TimeSpan.FromMilliseconds(5), attempts[0].Elapsed);
    }
    [TestMethod]
    public void Buffer_WritesFullBatch_AndFlushesPartialBatchOnDispose()
    {
        var target = new RecordingSink(); var buffer = new BufferedLogSink(target, 2);
        buffer.Write(Samples.Record("a"), "a"); Assert.IsEmpty(target.Texts);
        buffer.Write(Samples.Record("b"), "b"); Assert.HasCount(2, target.Texts);
        buffer.Write(Samples.Record("c"), "c"); Assert.AreEqual(1, buffer.PendingCount);
        buffer.Dispose(); buffer.Dispose();
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, target.Texts); Assert.AreEqual(1, target.DisposeCount);
    }
    [TestMethod]
    public void Buffer_AfterFailureDoesNotReplaySuccessfulPrefix()
    {
        var target = new RecordingSink { FailOnAttempt = 2 }; using var buffer = new BufferedLogSink(target, 3);
        buffer.Write(Samples.Record("a"), "a"); buffer.Write(Samples.Record("b"), "b");
        Assert.ThrowsExactly<BufferedWriteException>(() => buffer.Write(Samples.Record("c"), "c"));
        Assert.HasCount(1, target.Texts); Assert.AreEqual(2, buffer.PendingCount);
        buffer.Flush(); CollectionAssert.AreEqual(new[] { "a", "b", "c" }, target.Texts);
    }
    [TestMethod]
    public void RetryOutsideBuffer_ResumesFlushWithoutEnqueuingCurrentRecordTwice()
    {
        var target = new RecordingSink { FailOnAttempt = 2 };
        using var retry = new RetryLogSink(new BufferedLogSink(target, 2), 3);
        retry.Write(Samples.Record("a"), "a"); retry.Write(Samples.Record("b"), "b");
        Assert.AreEqual(3, target.Attempts); CollectionAssert.AreEqual(new[] { "a", "b" }, target.Texts);
    }
    [TestMethod]
    [DataRow("buffer,retry,timing")]
    [DataRow("buffer,timing,retry")]
    [DataRow("retry,buffer,timing")]
    [DataRow("retry,timing,buffer")]
    [DataRow("timing,buffer,retry")]
    [DataRow("timing,retry,buffer")]
    public void AllSixDecoratorOrders_DeliverEveryRecordOnceAfterTransientFailure(string order)
    {
        var target = new RecordingSink { RemainingFailures = 1 }; var reports = new List<WriteMeasurement>();
        var options = new DeliveryOptions { Decorators = order.Split(',').ToList(), BatchSize = 2 };
        using (var sink = SinkDecoration.Apply(target, options, new ManualClock(), reports.Add))
            for (var i = 0; i < 5; i++) sink.Write(Samples.Record(i.ToString()), i.ToString());
        CollectionAssert.AreEqual(new[] { "0", "1", "2", "3", "4" }, target.Texts);
        Assert.AreEqual(1, target.DisposeCount);
    }
    [TestMethod]
    public void Buffer_DisposesInnerEvenWhenFinalFlushFails()
    {
        var target = new RecordingSink { FailAlways = true }; var buffer = new BufferedLogSink(target, 2);
        buffer.Write(Samples.Record(), "x");
        Assert.ThrowsExactly<IOException>(() => buffer.Dispose());
        Assert.AreEqual(1, target.DisposeCount);
    }
    [TestMethod]
    public void DecorationRejectsUnknownNames()
    {
        using var target = new RecordingSink();
        var options = new DeliveryOptions { Decorators = new() { "unknown" } };
        Assert.ThrowsExactly<ArgumentException>(() => SinkDecoration.Apply(target, options, new ManualClock(), _ => { }));
    }
    [TestMethod]
    public void DecorationRejectsRepeatedNames()
    {
        using var target = new RecordingSink();
        var options = new DeliveryOptions { Decorators = new() { "retry", "retry" } };
        Assert.ThrowsExactly<ArgumentException>(() => SinkDecoration.Apply(target, options, new ManualClock(), _ => { }));
    }
}
