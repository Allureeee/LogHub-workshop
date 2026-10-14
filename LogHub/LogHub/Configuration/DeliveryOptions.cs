namespace LogHub.Configuration;
public sealed class RetryOptions
{
    public int Attempts { get; set; } = 3;
    public RetryOptions DeepClone() => (RetryOptions)MemberwiseClone();
}
public sealed class DeliveryOptions
{
    public List<string> Decorators { get; set; } = new();
    public int BatchSize { get; set; } = 4;
    public RetryOptions Retry { get; set; } = new();
    public DeliveryOptions DeepClone() => new()
    {
        Decorators = new List<string>(Decorators), BatchSize = BatchSize, Retry = Retry.DeepClone()
    };
}
