﻿using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Npgsql;

namespace SV.Maat.lib.Repository
{
    public class RepositoryBase<T> : IRepository<T> where T : Model
    {
        public RepositoryBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected string _connectionString;

        protected IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public T Find(long id)
        {
            using (Connection)
            {
                return Connection.Get<T>(id);
            }
        }

        public IEnumerable<T> Get()
        {
            using (Connection)
            {
                return Connection.GetAll<T>();
            }
        }

        public long Insert(T model)
        {
            using (Connection)
            {
                return Connection.Insert<T>(model);
            }
        }

        public bool Update(T model)
        {
            using (Connection)
            {
                return Connection.Update<T>(model);
            }
        }

        public bool Delete(long id)
        {
            using (Connection)
            {
                var model = Find(id);
                return Connection.Delete<T>(model);
            }
        }

    }
}
