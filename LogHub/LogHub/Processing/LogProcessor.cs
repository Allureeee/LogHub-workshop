using LogHub.Importing;
namespace LogHub.Processing;
public sealed class LogProcessor
{
    private readonly LogFlowMediator _mediator;
    public LogProcessor(LogFlowMediator mediator) => _mediator = mediator;
    public void ChangeImporter(ILogImporter importer) => _mediator.ChangeImporter(importer);
    public ProcessingResult Run() => _mediator.Run();
}
