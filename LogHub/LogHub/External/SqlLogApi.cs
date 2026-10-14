namespace LogHub.External;
public class SqlLogApi : IDisposable
{
    private readonly ISqlSession _session;
    public SqlLogApi(ISqlSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        try { _session.Open(); }
        catch { _session.Dispose(); throw; }
    }
    public void Save(DateTime date, int severity, string message) => _session.Save(date, severity, message);
    public void Dispose() => _session.Dispose();
}
