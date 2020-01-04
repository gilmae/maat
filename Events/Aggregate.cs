using System;
namespace Events
{
    public class Aggregate
    {
        public Guid Id { get; set; }

        public Aggregate() : this(Guid.NewGuid())
        {
            
        }

        public Aggregate(Guid id)
        {
            Id = id;
        }
    }
}
