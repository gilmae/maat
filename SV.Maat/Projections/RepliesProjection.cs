using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Projections
{
    public class RepliesProjection : ProjectionBase, IRepliesProjection
    {
        ConcurrentDictionary<string, List<Guid>> repliesProjections = new ConcurrentDictionary<string, List<Guid>>();
        ConcurrentDictionary<Guid, string> isReplyIndex = new ConcurrentDictionary<Guid, string>();

        public RepliesProjection(ILogger logger, IEventStore eventStore) : base(logger, eventStore) { }

        public IEnumerable<Guid> GetReplyIds(string url)
        {
            if (repliesProjections.TryGetValue(url,  out var replies))
            {
                return replies;
            }
            return new Guid[] { };
        }

        public override (int?, int) ProcessEvents(IList<Event> newEvents)
        {
            if (newEvents.Any())
            {
                var entryEvents = newEvents.Where(e => e is Event<Entry>);
                logger.LogTrace("Processing {0} new Entry events", entryEvents.Count());
                foreach (ReplyingTo e in entryEvents.Where(e => e is ReplyingTo))
                {
                    List<Guid> replies;
                   
                    if (!string.IsNullOrEmpty(e.ReplyTo))
                    {
                        if (isReplyIndex.TryGetValue(e.AggregateId, out string oldReplyTo) && oldReplyTo != e.ReplyTo)
                        {
                            if (repliesProjections.TryGetValue(oldReplyTo, out replies))
                            {
                                replies.RemoveAll(x => x == e.AggregateId);
                                repliesProjections.AddOrUpdate(oldReplyTo, replies, (key, oldValue) => replies);
                            }
                        }

                        if (!repliesProjections.TryGetValue(e.ReplyTo, out replies))
                        {
                            replies = new List<Guid>();
                        }
                        replies.Add(e.AggregateId);
                        replies = replies.Distinct().ToList();
                        repliesProjections.AddOrUpdate(e.ReplyTo, replies, (key, oldValue) => replies);
                        isReplyIndex.AddOrUpdate(e.AggregateId, e.ReplyTo, (key, oldValue) => e.ReplyTo);
                    }
                    else
                    {
                        if (isReplyIndex.TryGetValue(e.AggregateId, out string oldReplyTo))
                        {
                            if (repliesProjections.TryGetValue(oldReplyTo, out replies))
                            {
                                replies.RemoveAll(x => x == e.AggregateId);
                                repliesProjections.AddOrUpdate(oldReplyTo, replies, (key, oldValue) => replies);
                            }
                        }
                    }
                }
                int maxId = newEvents.Max(e => e.Id);
                logger.LogTrace("Replies projection updated, last id processed: {0}", maxId);
                return (maxId, 0);
            }

            return (null, 5);
        }
    }
}
