using System;
using System.Collections.Generic;

namespace Events
{
    public interface IProjection<T, Tid>
    {
        T Get(Tid id);
        IEnumerable<T> Get();
       
    }
}
