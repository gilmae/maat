using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using Nancy.Authentication.Stateless;
using StrangeVanilla.Maat.lib.MessageBus;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubModule : Nancy.NancyModule
    {
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;
        IMessageBus<Entry> _entryBus;
        public MicropubModule(ILogger<NancyModule> logger, IEventStore<Entry> entryRepository, IEventStore<Media> mediaRepository, IMessageBus<Entry> entryBus)
        {

            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            StatelessAuthentication.Enable(this, IndieAuth.GetAuthenticationConfiguration());
            this.RequiresAuthentication();

            Get("/micropub/{id:Guid}",

                p =>
                {
                    var entry = new Entry((Guid)p.id);
                    var events = _entryRepository.Retrieve(p.id);

                    foreach (var e in events)
                    {
                        entry = e.Apply(entry);
                    }

                    return Newtonsoft.Json.JsonConvert.SerializeObject(entry);
                }
                );

            Post("/micropub",
                p => {

                    var post = this.Bind<MicropubPost>();
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

                    if (this.Request.Files != null)
                    {

                        events.AddRange(this.Request.Files.Select(ProcessMediaFile).Select(m => new MediaAssociated(entry, m)));
                    }

                    _entryRepository.StoreEvent(events);
                    foreach (var e in events)
                    {
                        entry = e.Apply(entry);
                    }

                    _entryBus.Publish(entry.Id);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(entry);

                }
            );

            Post("/micropub/media",
                p =>
                {
                   
                    if (this.Request.Files != null)
                    {
                        var media = this.Request.Files.Select(f=>ProcessMediaFile(f));
                        return Newtonsoft.Json.JsonConvert.SerializeObject(media);
                    }
                    return null;
                });
        }

        private Media ProcessMediaFile(HttpFile file)
        {
            if (file.Value != null && file.Value.Length != 0)
            {
                string savePath = Path.GetFullPath(".");

                // Put it in the MEdia STore
                //using (var reader = new StreamReader(file.Value))
                //{
                //    var bytes = reader.ReadToEnd();
                //    File.WriteAllBytes(savePath, bytes);

                //}
                var m = new Media();
                MediaUploaded e = new MediaUploaded
                {
                    Name = file.Name,
                    MediaStoreId = string.Format("{1}/{0}", Guid.NewGuid(), savePath),
                    MimeType = file.ContentType
                };
                e.Apply(m);
                _mediaRepository.StoreEvent(e);
                return m;
            }
            return null;
        }
    }
}
