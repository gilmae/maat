using System;
using System.Data;
using Npgsql;

namespace SimpleRepo
{
    public class PgContext : IDbContext
    {
        string _connectionString;
        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}
