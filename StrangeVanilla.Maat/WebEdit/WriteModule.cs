using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.Commands;

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

                CreateEntryCommand command = new CreateEntryCommand(_entryRepository);
                ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository);

                var entry = command.Execute(post.name,
                    post.content,
                    post.category,
                    null
                );

                return new RedirectResponse("/write");
            });
        }
    }
}
