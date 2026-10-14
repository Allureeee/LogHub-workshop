namespace LogHub.Configuration;
public class LogConfiguration
{
    public string Sink { get; set; } = "console,file";
    public string Importer { get; set; } = "mixed";
    public string MinSeverity { get; set; } = "Info";
    public string Format { get; set; } = "plain";
    public string Connection { get; set; } = "local";
    public string LogPath { get; set; } = "sample.log";
    public string OutputPath { get; set; } = string.Empty;
    public string Filters { get; set; } = "severity,age,duplicates";
    public int MaxAgeMinutes { get; set; } = 60;
    public string Notifications { get; set; } = "push";
    public int BufferCapacity { get; set; } = 4;
    public DateTimeOffset? ReferenceTimeUtc { get; set; }

    // "legacy" keeps the explicitly selected LR1-LR4 profiles unchanged.
    public string Family { get; set; } = "legacy";
    public string Builder { get; set; } = "fluent";
    public string Adapter { get; set; } = "object";
    public string DatabasePath { get; set; } = "loghub.sqlite";
    public string DocumentPath { get; set; } = "documents.jsonl";
    public string RuleJson { get; set; } = string.Empty;
    public DeliveryOptions Delivery { get; set; } = new();
    public LogConfiguration ShallowClone() => (LogConfiguration)MemberwiseClone();
    public LogConfiguration DeepClone()
    {
        var clone = ShallowClone();
        clone.Delivery = Delivery.DeepClone();
        return clone;
    }
}
