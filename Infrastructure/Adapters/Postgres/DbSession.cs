using System.Data;

namespace Infrastructure.Adapters.Postgres;

public class DbSession : IDisposable
{
    private Guid _id;

    public DbSession(IDbConnection connection)
    {
        _id = Guid.NewGuid();
        Connection = connection;
        Connection.Open();
    }
    
    public IDbConnection Connection { get; }
    
    public IDbTransaction Transaction { get; set; } = null!;

    
    public void Dispose() => Connection.Dispose();
}