using System;
namespace Events
{
    public interface IProjection<T> where T: Aggregate
    {
        T Get(Guid id);

    }
}
