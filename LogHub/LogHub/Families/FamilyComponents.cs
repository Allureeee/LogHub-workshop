namespace LogHub.Families;
public sealed class FamilyComponents
{
    public FamilyOutput Output { get; }
    public FamilyStorage Storage { get; }
    public FamilyFormatter Formatter { get; }
    public FamilyComponents(FamilyOutput output, FamilyStorage storage, FamilyFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(formatter);
        if (output.Family != storage.Family || output.Family != formatter.Family)
            throw new ArgumentException("Output, storage and format must belong to the same family.");
        Output = output; Storage = storage; Formatter = formatter;
    }
}
