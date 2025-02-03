using Npgsql;

namespace Infrastructure.Adapters.Postgres;

public class DbSession(NpgsqlDataSource dataSource) : IDisposable
{
    private Guid _id = Guid.NewGuid();
    
    public NpgsqlConnection Connection { get; } = dataSource.OpenConnection();

    public NpgsqlTransaction? Transaction { get; set; }
    
    public void Dispose()
    {
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }
}