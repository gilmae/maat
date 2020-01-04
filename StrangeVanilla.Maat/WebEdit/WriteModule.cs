using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat
{
    public class WriteModule : NancyModule
    {
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;

        public WriteModule(ILogger<NancyModule> logger, IEventStore<Entry> entryRepository, IEventStore<Media> mediaRepository)
        {
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            Get("/write", p =>
            {
                return View["WebEdit/New.html"];
            });

            Post("/write/create", p => {

                var post = this.Bind<Micropub.MicropubPost>();
                var entry = new Entry();
                var events = new List<Event<Entry>>();

                events.Add(new EntryAdded(entry)
                {
                    Body = post.content,
                    Title = post.name
                });

                if (post.category != null)
                {
                    events.AddRange(post.category.Select(c => new EntryCategorised(entry, c)));
                }
                _entryRepository.StoreEvent(events);
                foreach (var e in events)
                {
                    entry = e.Apply(entry);
                }

                return new RedirectResponse("/write");
            });
        }
    }
}
