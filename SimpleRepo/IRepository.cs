using System;
using System.Collections.Generic;

namespace SimpleRepo
{
    public interface IRepository<T> where T : Model
    {
         abstract T Find(long id);
         abstract IEnumerable<T> Get();
         abstract long Insert(T model);
         abstract bool Update(T model);
         abstract bool Delete(long id);
    }
}
