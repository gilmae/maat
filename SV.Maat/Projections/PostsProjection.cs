using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Projections
{
    public interface IPostsProjection
    {
        Post Get(Guid id);
        Post Get(string slug);
        IEnumerable<Post> Get();
        IEnumerable<Post> GetAfter(int numItems, DateTime after);
        IEnumerable<Post> GetBefore(int numItems, DateTime before);
        IEnumerable<Post> GetLatest(int numItems);
    }

    public class PostsProjection : ProjectionBase, IPostsProjection
    {
        ConcurrentDictionary<Guid, Post> projections = new ConcurrentDictionary<Guid, Post>();
        public PostsProjection(ILogger<PostsProjection> logger, IEventStore<Post> eventStore) : base(logger, eventStore)
        {
        }

        public Post Get(Guid id)
        {
            if (projections.TryGetValue(id, out Post p))
            {
                return p;
            }
            return null;
        }
    

        public Post Get(string slug)
        {
            return projections.Values.FirstOrDefault(e => e.Data.Properties.ContainsKey("slug")
                && e.Data.Properties["slug"].Any(p=>p as string == slug));
        }

        public IEnumerable<Post> Get()
        {
            return projections.Values.OrderByDescending(e => e.CreatedAt);
        }

        public IEnumerable<Post> GetAfter(int numItems, DateTime after)
        {
            return Get().Where(x => x.CreatedAt > after).Take(numItems);
        }

        public IEnumerable<Post> GetBefore(int numItems, DateTime before)
        {
            return Get().Where(x => x.CreatedAt < before).Take(numItems);
        }

        public IEnumerable<Post> GetLatest(int numItems)
        {
            return Get().Take(numItems);
        }

        public override (int?, int) ProcessEvents(IList<Event> newEvents)
        {
            if (newEvents.Any())
            {
                var entryEvents = newEvents.Where(e => e is Event<Post>);
                logger.LogTrace("Processing {0} new Post events", entryEvents.Count());
                foreach (Event<Post> e in entryEvents.Where(e => e is Event<Post>))
                {
                    if (!projections.TryGetValue(e.AggregateId, out Post aggregate))
                    {
                        aggregate = new Post(e.AggregateId);
                    }
                    e.Apply(aggregate);
                    projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
                }
                int maxId = newEvents.Max(e => e.Id);
                logger.LogTrace("Post projection updated, last id processed: {0}", maxId);
                return (maxId, 0);
            }

            return (null, 5);
        }
    }
}
