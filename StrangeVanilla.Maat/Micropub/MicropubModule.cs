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
using System.Collections.Generic;

namespace StrangeVanilla.Maat.Micropub
{
    public partial class MicropubModule : NancyModule
    {
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;
        IMessageBus<Entry> _entryBus;
        IFileStore _fileStore;

        public MicropubModule(ILogger<NancyModule> logger,
            IEventStore<Entry> entryRepository,
            IEventStore<Media> mediaRepository,
            IMessageBus<Entry> entryBus,
            IFileStore fileStore)
        {

            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            _fileStore = fileStore;

            StatelessAuthentication.Enable(this, IndieAuth.GetAuthenticationConfiguration());
            this.RequiresAuthentication();



            Post("/micropub",
                p =>
                {
                    MicropubPayload post;
                    try
                    {
                        post = this.Bind<MicropubPayload>();
                    }
                    catch (Exception)
                    {
                        return new Nancy.Responses.HtmlResponse(HttpStatusCode.BadRequest);
                    }

                    if (post.IsCreate())
                    {
                        return Create(post);
                    }
                    else
                    {
                        return Update(post);
                    }
                }
            );
        }

        public Response Create(MicropubPayload post)
        {
            CreateEntryCommand command = new CreateEntryCommand(_entryRepository);
            ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository, _fileStore);

            var entry = command.Execute(post.Properties.GetValueOrDefault("name")?[0],
                post.Properties.GetValueOrDefault("content")?[0],
                post.Properties.GetValueOrDefault("category"),
                post.Properties.GetValueOrDefault("bookmark-of")?[0],
                this.Request.Files.Select(f => mediaProcessor.Execute(f.Name, f.ContentType, f.Value)),
                post.Properties.GetValueOrDefault("post-status")?[0] != "draft"
            );

            _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });

            var response = new Nancy.Responses.TextResponse() { StatusCode = HttpStatusCode.Created };

            response.Headers.Add("Location", this.Context.EntryUrl(entry));
            return response;

        }

        public Response Update(MicropubPayload post)
        {
            Response response;
            Entry entry;

            if (string.IsNullOrEmpty(post.Url))
            {
                response = new Nancy.Responses.JsonResponse(new
                {
                    error = "invalid_request",
                    error_description = "URL was not provided"
                }, new Nancy.Serialization.JsonNet.JsonNetSerializer(), this.Context.Environment);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            Guid entryId = this.Context.GetEntryIdFromUrl(post.Url);

            if (entryId == Guid.Empty)
            {
                response = new Nancy.Responses.JsonResponse(new
                {
                    error = "invalid_request",
                    error_description = "URL could not be parsed."
                }, new Nancy.Serialization.JsonNet.JsonNetSerializer(), this.Context.Environment);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }
            else
            {
                entry = new Entry(entryId);

                if (post.Add?.Count() > 0)
                {
                    HandleAddUpdates(post, entry);
                }
                if (post.Replace?.Count() > 0)
                {
                    HandleReplaceUpdates(post, entry);
                }
                else if (post.Remove?.Count() > 0)
                {
                    HandleRemoveUpdates(post, entry);

                }
                _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });

                response = new Nancy.Responses.TextResponse() { StatusCode = HttpStatusCode.Created };

                response.Headers.Add("Location", this.Context.EntryUrl(entry));
                return response;
            }
        }

        private void HandleRemoveUpdates(MicropubPayload post, Entry entry)
        {
            var removeCommand = new UpdateEntryAsRemoveCommand(_entryRepository);
            removeCommand.Execute(entry, post.Remove.Contains("name"),
                post.Remove.Contains("content"),
                post.Remove.Contains("category"),
                post.Remove.Contains("bookmark-of"),
                false,
                false
            );
        }

        private void HandleReplaceUpdates(MicropubPayload post, Entry entry)
        {
            var replaceCommand = new UpdateEntryAsReplaceCommand(_entryRepository);
            replaceCommand.Execute(entry, post.Replace.GetValueOrDefault("name")?[0],
        post.Replace.GetValueOrDefault("content")?[0],
        post.Replace.GetValueOrDefault("category"),
        post.Replace.GetValueOrDefault("bookmark-of")?[0],
                null,
                post.Replace.GetValueOrDefault("post-status")?[0] != "draft"
            );
        }

        private void HandleAddUpdates(MicropubPayload post, Entry entry)
        {
            var addCommand = new UpdateEntryAsAddCommand(_entryRepository);
            addCommand.Execute(entry, post.Add.GetValueOrDefault("name")?[0],
        post.Add.GetValueOrDefault("content")?[0],
        post.Add.GetValueOrDefault("category"),
        post.Add.GetValueOrDefault("bookmark-of")?[0],
                null,
                post.Add.GetValueOrDefault("post-status")?[0] != "draft"
            );
        }
    }

}
