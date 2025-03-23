using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace BankingApp.Infrastructure.Config
{
    public class ConnectionFactory
    {
        private readonly string _connectionString;

        public ConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres");
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
