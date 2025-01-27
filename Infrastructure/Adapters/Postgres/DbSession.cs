using System.Data;
using System.Data.Common;

namespace Infrastructure.Adapters.Postgres;

public class DbSession(DbDataSource dataSource) : IDisposable
{
    private Guid _id = Guid.NewGuid();
    
    public IDbConnection Connection { get; } = dataSource.OpenConnection();

    public IDbTransaction? Transaction { get; set; }
    
    public void Dispose()
    {
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }
}