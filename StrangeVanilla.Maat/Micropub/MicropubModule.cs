﻿using System;
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

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubModule : NancyModule
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

            

            Post("/micropub",
                p => {
                    var post = this.Bind<MicropubPost>();

                    CreateEntryCommand command = new CreateEntryCommand(_entryRepository);
                    ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository);

                    var entry = command.Execute(post.name,
                        post.content,
                        post.category,
                        this.Request.Files.Select(f => mediaProcessor.Execute(f.Name, f.ContentType, f.Value)),
                        post.postStatus != "draft"
                    );
                    _entryBus.Publish(entry.Id);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(entry);
                }
            );

            Post("/micropub/media",
                p =>
                {
                    ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository);
                    if (this.Request.Files != null)
                    {
                        var media = this.Request.Files.Select(f => mediaProcessor.Execute(f.Name, f.ContentType, f.Value)
);
                        return Newtonsoft.Json.JsonConvert.SerializeObject(media);
                    }
                    return null;
                });
        }

    }
}
