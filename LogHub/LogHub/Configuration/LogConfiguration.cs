namespace LogHub.Configuration;
public class LogConfiguration
{
    public string Sink { get; set; } = "console";
    public string MinSeverity { get; set; } = "Info";
    public string Format { get; set; } = "plain";
    public string Connection { get; set; } = "local";
    public string LogPath { get; set; } = "sample.log";
}