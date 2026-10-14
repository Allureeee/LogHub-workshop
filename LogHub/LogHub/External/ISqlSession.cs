namespace LogHub.External;
public interface ISqlSession : IDisposable
{
    void Open();
    void Save(DateTime date, int severity, string message);
}
