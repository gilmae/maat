using System;
using System.IO;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using StrangeVanilla.Blogging.Events;
using Nancy.Authentication.Stateless;
using StrangeVanilla.Maat.lib.MessageBus;
using StrangeVanilla.Maat.Commands;
using StrangeVanilla.Maat.lib;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubModule : NancyModule
    {
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;
        IMessageBus<Entry> _entryBus;
        public MicropubModule(ILogger<NancyModule> logger,
            IEventStore<Entry> entryRepository,
            IEventStore<Media> mediaRepository,
            IMessageBus<Entry> entryBus,
            IFileStore fileStore)
        {

            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            StatelessAuthentication.Enable(this, IndieAuth.GetAuthenticationConfiguration());
            this.RequiresAuthentication();

            

            Post("/micropub",
                p => {
                    var post = this.Bind<MicropubPost>();

                    CreateEntryCommand command = new CreateEntryCommand(_entryRepository);
                    ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository, fileStore);

                    var entry = command.Execute(post.Title,
                        post.Content,
                        post.Categories,
                        post.BookmarkOf,
                        this.Request.Files.Select(f => mediaProcessor.Execute(f.Name, f.ContentType, f.Value)),
                        post.PostStatus != "draft"
                    );
                    _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });

                    var response = new Nancy.Responses.TextResponse() { StatusCode = HttpStatusCode.Created };
                    
                    response.Headers.Add("Location", this.Context.EntryUrl(entry));
                    return response;
                }
            );
        }

    }
}
