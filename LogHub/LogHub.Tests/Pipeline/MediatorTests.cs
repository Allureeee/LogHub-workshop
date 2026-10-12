using System.Reflection;
using LogHub.Configuration;
using LogHub.Importing;
using LogHub.Processing;
namespace LogHub.Tests.Pipeline;

[TestClass]
public sealed class MediatorTests
{
    [TestMethod]
    public void PausedImporter_DoesNotAdvanceItsUnderlyingEnumerator()
    {
        var importer = new TrackingImporter(3);
        using var session = new ImportSession(importer);
        session.Start(); Assert.IsTrue(session.TryRead(out _));
        session.Pause(); Assert.IsFalse(session.TryRead(out _));
        Assert.AreEqual(1, importer.Yielded);
        session.Resume(); Assert.IsTrue(session.TryRead(out _)); Assert.AreEqual(2, importer.Yielded);
    }
    [TestMethod]
    public void FullBuffer_NotifiesMediator_WhichPausesImporter()
    {
        using var rig = new PipelineRig("", new LogConfiguration { BufferCapacity = 1 });
        rig.Pipeline.Enqueue(new ImportItem(1, TestPipeline.Entry()));
        Assert.IsTrue(rig.Session.Paused); Assert.AreEqual(1, rig.Mediator.PauseCount);
        rig.Pipeline.Drain(); Assert.IsFalse(rig.Session.Paused); Assert.AreEqual(1, rig.Mediator.ResumeCount);
    }
    [TestMethod]
    public void CooperativeBackpressure_DoesNotDropTheLastIncompleteBatch()
    {
        var importer = new TrackingImporter(5);
        using var rig = new PipelineRig("", new LogConfiguration { BufferCapacity = 2 }, importer);
        Assert.AreEqual(new ProcessingResult(5, 0), rig.Processor.Run());
        Assert.AreEqual(2, rig.Mediator.PauseCount); Assert.AreEqual(2, rig.Mediator.ResumeCount);
        Assert.AreEqual(5, importer.Yielded); Assert.IsTrue(importer.Released);
        Assert.AreEqual(2, rig.Pipeline.MaximumBuffered); Assert.AreEqual(0, rig.Pipeline.PendingCount);
    }
    [TestMethod]
    public void Pipeline_HasNoImporterOrImportSessionField()
    {
        foreach (var field in typeof(ProcessingPipeline).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            Assert.IsFalse(typeof(ILogImporter).IsAssignableFrom(field.FieldType));
            Assert.AreNotEqual(typeof(ImportSession), field.FieldType);
        }
    }
    [TestMethod]
    public void SourceFailure_StillReleasesEnumerator_AndClearsPendingBuffer()
    {
        var importer = new TrackingImporter(5, failAt: 2);
        using var rig = new PipelineRig("", new LogConfiguration { BufferCapacity = 4 }, importer);
        TestPipeline.Throws<IOException>(() => rig.Processor.Run());
        Assert.IsTrue(importer.Released); Assert.AreEqual(0, rig.Pipeline.PendingCount);
        Assert.IsFalse(rig.Session.Paused);
    }
    [TestMethod]
    public void ZeroCapacity_IsRejected() =>
        TestPipeline.Throws<ArgumentOutOfRangeException>(() => new PipelineRig("", new LogConfiguration { BufferCapacity = 0 }));
    [TestMethod]
    public void EmptySource_ProducesZeroCounts_AndNoPauses()
    {
        using var rig = new PipelineRig("");
        Assert.AreEqual(new ProcessingResult(0, 0), rig.Processor.Run());
        Assert.AreEqual(0, rig.Mediator.PauseCount); Assert.AreEqual(0, rig.Counter.Count);
    }
    [TestMethod]
    public void ChangingStrategyDuringActiveSession_IsRejected()
    {
        using var session = new ImportSession(new TrackingImporter(2)); session.Start();
        TestPipeline.Throws<InvalidOperationException>(() => session.ChangeImporter(new TrackingImporter(3)));
    }
    private sealed class TrackingImporter : ILogImporter
    {
        private readonly int _count;
        private readonly int _failAt;
        public int Yielded { get; private set; }
        public bool Released { get; private set; }
        public TrackingImporter(int count, int failAt = -1) => (_count, _failAt) = (count, failAt);
        public IEnumerable<ImportItem> Import()
        {
            try
            {
                for (var i = 0; i < _count; i++)
                {
                    if (i == _failAt) throw new IOException("Injected source failure.");
                    Yielded++; yield return new ImportItem(i + 1, TestPipeline.Entry("record-" + i));
                }
            }
            finally { Released = true; }
        }
    }
}
