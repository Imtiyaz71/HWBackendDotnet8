using System.Data;
using Microsoft.Data.SqlClient;
using Som_Service.Interface;

namespace Som_Infrastructure.Data
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                builder.TrustServerCertificate = true;
                _connectionString = builder.ConnectionString;
            }
            catch
            {
                _connectionString = connectionString;
            }
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
