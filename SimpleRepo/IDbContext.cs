using System;
using System.Data;

namespace SimpleRepo
{
    public interface IDbContext
    {
        void SetConnectionString(string connectionString);
        IDbConnection GetConnection();
    }
}
