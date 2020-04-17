using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace SV.Maat.lib.Repository
{
    public class RepositoryBase<T> : IRepository<T> where T: Model
    {
        protected readonly IDbConnection _connection;
        public T Find(long id)
        {
            return _connection.Get<T>(id);
        }

        public IEnumerable<T> Get()
        {
            return _connection.GetAll<T>();
        }

        public long Insert(T model)
        {
            return _connection.Insert<T>(model);
        }

        public  bool Update(T model)
        {
            return _connection.Update<T>(model);
        }

        public  bool Delete(long id)
        {
            var model = Find(id);
            return _connection.Delete<T>(model);
        }

    }
}
