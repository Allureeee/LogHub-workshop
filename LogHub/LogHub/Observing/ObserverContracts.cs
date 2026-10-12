using LogHub.Models;
namespace LogHub.Observing;

public sealed record ProcessedLog(int Sequence, LogRecord Record, string Text);
public sealed record ObserverFailure(string Observer, Exception Error);
public interface IPushLogObserver { void OnProcessed(ProcessedLog item); }
public interface IPullLogObserver { void OnChanged(IPullLogSource source); }
public interface IPullLogSource { ProcessedLog? Current { get; } }
