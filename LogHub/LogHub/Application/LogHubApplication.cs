using LogHub.Configuration;
using LogHub.Processing;

namespace LogHub.Application;
public class LogHubApplication
{
    private readonly IConfigService _configService;
    private readonly LogProcessingModule _processingModule;
    public LogHubApplication(
        IConfigService configService,
        LogProcessingModule processingModule)
    {
        _configService = configService;
        _processingModule = processingModule;
    }
    public ProcessingResult Run()
    {
        var logPath = _configService.GetValue("logPath") ?? "sample.log";
        return _processingModule.Run(logPath);
    }
}