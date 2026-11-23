using System.Data;

namespace Som_Service.Interface
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
    }
}
