using LogHub.Configuration;
using LogHub.Observing;
using LogHub.Sinks;
namespace LogHub.Tests.Pipeline;

[TestClass]
public sealed class ObserverTests
{
    private static ProcessedLog Item(int n = 1, string severity = "Info") =>
        new(n, TestPipeline.Entry("record-" + n, severity), "formatted-" + n);
    [TestMethod]
    [DataRow("push")]
    [DataRow("pull")]
    public void UnsubscribedObserver_ReceivesNoFurtherEvents(string mode)
    {
        var events = new PipelineEvents(); var observer = new ProcessedCounterObserver();
        var subscription = mode == "push" ? events.SubscribePush(observer) : events.SubscribePull(observer);
        events.Publish(Item()); subscription.Dispose(); subscription.Dispose(); events.Publish(Item(2));
        Assert.AreEqual(1, observer.Count);
    }
    [TestMethod]
    [DataRow("push")]
    [DataRow("pull")]
    public void ThrowingObserver_DoesNotBlockTheRemainingObservers(string mode)
    {
        var events = new PipelineEvents(); var counter = new ProcessedCounterObserver();
        using var subscriptions = new PipelineSubscriptions(events, new LogConfiguration { Notifications = mode },
            new LogObserver[] { new ThrowingObserver(), counter });
        events.Publish(Item()); events.Publish(Item(2));
        Assert.AreEqual(2, counter.Count); Assert.HasCount(2, events.Failures);
    }
    [TestMethod]
    public void Push_ProvidesThePayloadDirectly()
    {
        var events = new PipelineEvents(); var observer = new PushCollector();
        using var token = events.SubscribePush(observer); var item = Item();
        events.Publish(item); Assert.AreSame(item, observer.Items.Single());
    }
    [TestMethod]
    public void Pull_NotifiesThenReadsTheSourcesCurrentRecord()
    {
        var events = new PipelineEvents(); var observer = new PullCollector();
        using var token = events.SubscribePull(observer); var item = Item();
        events.Publish(item); Assert.AreSame(item, observer.Items.Single());
        Assert.AreEqual(1, observer.Reads); Assert.AreSame(events, observer.Source);
    }
    [TestMethod]
    public void PushAndPull_ProduceIdenticalOrderedPayloads()
    {
        var events = new PipelineEvents(); var push = new PushCollector(); var pull = new PullCollector();
        using var a = events.SubscribePush(push); using var b = events.SubscribePull(pull);
        for (var i = 1; i <= 3; i++) events.Publish(Item(i));
        CollectionAssert.AreEqual(push.Items, pull.Items);
    }
    [TestMethod]
    public void CriticalObserver_AlertsOnlyOnCritical_CaseInsensitive()
    {
        var events = new PipelineEvents(); using var output = new StringWriter();
        var alerts = new CriticalAlertObserver(output); using var token = events.SubscribePush(alerts);
        events.Publish(Item(1, "Error")); events.Publish(Item(2, "critical"));
        Assert.AreEqual(1, alerts.AlertCount); StringAssert.Contains(output.ToString(), "formatted-2");
    }
    [TestMethod]
    public void ConsoleSubscriber_WritesToInjectedWriter_WithoutConsoleAccess()
    {
        var events = new PipelineEvents(); using var output = new StringWriter();
        using var sink = new ConsoleLogSink(new LogConfiguration(), output);
        using var token = events.SubscribePush(new SinkObserver<ConsoleLogSink>(sink));
        events.Publish(Item()); Assert.AreEqual("formatted-1" + Environment.NewLine, output.ToString());
    }
    [TestMethod]
    public void DisposingSubscriptions_RemovesAllObservers()
    {
        var events = new PipelineEvents(); var counter = new ProcessedCounterObserver();
        var subscriptions = new PipelineSubscriptions(events, new LogConfiguration(), new[] { counter });
        subscriptions.Dispose(); events.Publish(Item()); Assert.AreEqual(0, counter.Count);
    }
    [TestMethod]
    public void ReentrantPublication_IsIsolated_AndPullSnapshotStaysCorrect()
    {
        var events = new PipelineEvents(); var pull = new PullCollector();
        using var a = events.SubscribePush(new ReentrantObserver(events));
        using var b = events.SubscribePull(pull);
        var original = Item(); events.Publish(original);
        Assert.AreSame(original, pull.Items.Single()); Assert.HasCount(1, events.Failures);
    }
    [TestMethod]
    public void UnsubscribeDuringPublication_TakesEffectBeforeRemainingDelivery()
    {
        var events = new PipelineEvents(); var counter = new ProcessedCounterObserver();
        IDisposable? second = null;
        using var first = events.SubscribePush(new ActionObserver(() => second!.Dispose()));
        second = events.SubscribePush(counter);
        events.Publish(Item()); Assert.AreEqual(0, counter.Count);
        second.Dispose();
    }
    private sealed class ThrowingObserver : LogObserver
    {
        protected override void Observe(ProcessedLog item) => throw new InvalidOperationException("subscriber failed");
    }
    private sealed class PushCollector : IPushLogObserver
    {
        public List<ProcessedLog> Items { get; } = new();
        public void OnProcessed(ProcessedLog item) => Items.Add(item);
    }
    private sealed class PullCollector : IPullLogObserver
    {
        public List<ProcessedLog> Items { get; } = new();
        public int Reads { get; private set; }
        public IPullLogSource? Source { get; private set; }
        public void OnChanged(IPullLogSource source)
        {
            Source = source; Reads++; Items.Add(source.Current!);
        }
    }
    private sealed class ReentrantObserver : LogObserver
    {
        private readonly PipelineEvents _events;
        public ReentrantObserver(PipelineEvents events) => _events = events;
        protected override void Observe(ProcessedLog item) => _events.Publish(Item(999));
    }
    private sealed class ActionObserver : LogObserver
    {
        private readonly Action _action;
        public ActionObserver(Action action) => _action = action;
        protected override void Observe(ProcessedLog item) => _action();
    }
}
