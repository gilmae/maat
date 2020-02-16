using System;
using System.IO;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.Security;
using StrangeVanilla.Blogging.Events;
using Nancy.Authentication.Stateless;
using StrangeVanilla.Maat.lib.MessageBus;
using StrangeVanilla.Maat.Commands;
using StrangeVanilla.Maat.lib;

namespace StrangeVanilla.Maat.Micropub
{
    public class MediaModule : NancyModule
    {
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;
        IMessageBus<Entry> _entryBus;
        public MediaModule(ILogger<NancyModule> logger, IEventStore<Entry> entryRepository, IEventStore<Media> mediaRepository, IMessageBus<Entry> entryBus, IFileStore fileStore)
        {

            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            StatelessAuthentication.Enable(this, IndieAuth.GetAuthenticationConfiguration());
            this.RequiresAuthentication();

            Post("/micropub/media",
                p =>
                {
                    ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository, fileStore);
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
