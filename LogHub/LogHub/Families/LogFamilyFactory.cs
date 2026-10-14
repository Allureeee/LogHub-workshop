namespace LogHub.Families;
public abstract class LogFamilyFactory
{
    public abstract FamilyOutput CreateOutput();
    public abstract FamilyStorage CreateStorage();
    public abstract FamilyFormatter CreateFormatter();
    public FamilyComponents CreateFamily()
    {
        var output = CreateOutput();
        FamilyStorage? storage = null;
        try
        {
            storage = CreateStorage();
            return new FamilyComponents(output, storage, CreateFormatter());
        }
        catch { try { output.Dispose(); } finally { storage?.Dispose(); } throw; }
    }
}
