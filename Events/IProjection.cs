using System;
using System.Collections.Generic;

namespace Events
{
    public interface IProjection<T> where T: Aggregate
    {
        T Get(Guid id);
        IEnumerable<T> Get();

    }
}
