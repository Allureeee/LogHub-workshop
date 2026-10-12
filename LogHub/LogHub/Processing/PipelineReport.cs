using LogHub.Observing;
using LogHub.Visiting;
namespace LogHub.Processing;

public sealed class PipelineReport
{
    private readonly StatisticsVisitor _statistics;
    private readonly ProcessedCounterObserver _counter;
    private readonly CriticalAlertObserver _alerts;
    private readonly LogFlowMediator _mediator;
    private readonly PipelineEvents _events;
    public PipelineReport(StatisticsVisitor statistics, ProcessedCounterObserver counter,
        CriticalAlertObserver alerts, LogFlowMediator mediator, PipelineEvents events)
    {
        _statistics = statistics; _counter = counter; _alerts = alerts;
        _mediator = mediator; _events = events;
    }
    public void Print(TextWriter output)
    {
        output.WriteLine($"Types: simple={_statistics.SimpleCount}; exception={_statistics.ExceptionCount}; metric={_statistics.MetricCount}");
        output.WriteLine($"Observer counter: {_counter.Count}; Critical alerts: {_alerts.AlertCount}");
        output.WriteLine($"Mediator: pauses={_mediator.PauseCount}; resumes={_mediator.ResumeCount}");
        foreach (var failure in _events.Failures)
            output.WriteLine($"[OBSERVER ERROR] {failure.Observer}: {failure.Error.Message}");
    }
}
