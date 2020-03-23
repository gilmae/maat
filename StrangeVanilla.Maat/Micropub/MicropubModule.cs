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
using StrangeVanilla.Blogging.Events.Entries.Events;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;

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
                    catch
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
            ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository, _fileStore);

            // Create can be a Form post, which may include uploaded files
            IEnumerable<Media> uploads = Request.Files.Select(f =>
                new ProcessMediaUpload(_mediaRepository, _fileStore)
                {
                    Name = f.Name,
                    MimeType = f.ContentType,
                    Stream = f.Value
                }
                .Execute()
            );

            IEnumerable<Entry.MediaLink> media = uploads.Select(u => new Entry.MediaLink { Url = this.Context.MediaUrl(u), Type = u.Name });

            var photos = ParseMediaReference(post.Properties.GetValueOrDefault("photo"), "photo");
            if (photos != null)
            {
                media = media.Union(photos).ToList();
            }

            string inReplyTo = post.Properties.GetValueOrDefault("in-reply-to")?[0]?.ToString();
            string postStatus = post.Properties.GetValueOrDefault("post-status")?[0]?.ToString();
            CreateEntryCommand command = new CreateEntryCommand(_entryRepository)
            {
                Content = post.Properties.GetValueOrDefault("content")?[0]?.ToString(),
                Name = post.Properties.GetValueOrDefault("name")?[0]?.ToString(),
                Categories = (post.Properties.GetValueOrDefault("category") as object[]).Select(x=>x.ToString()).ToArray(),
                Media = media,
                BookmarkOf = post.Properties.GetValueOrDefault("bookmark-of")?[0]?.ToString(),
                Published = postStatus == null || postStatus != "draft",
                ReplyTo = inReplyTo,
            };

            var entry = command.Execute();
            
            _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });

            var response = new Nancy.Responses.TextResponse() { StatusCode = HttpStatusCode.Created };

            response.Headers.Add("Location", UrlHelper.EntryUrl(Context, entry));
            return response;

        }

        private IEnumerable<Entry.MediaLink> ParseMediaReference(IEnumerable<dynamic> items, string type)
        {
            IList<Entry.MediaLink> media = new List<Entry.MediaLink>();
            foreach (dynamic item in items)
            {
                if (item is string)
                {
                    media.Add(new Entry.MediaLink { Url = item, Type=type });
                }
                else
                {
                    var m = new Entry.MediaLink { Url = item.value, Type = type };
                    try
                    {
                        m.Description = item.alt;
                    }
                    catch (RuntimeBinderException)
                    {

                    }
                    media.Add(m);
                }
            }
            return media;
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
                BaseCommand<Entry> command = null;
                try { 
                if (post.Add?.Count() > 0)
                {
                    command = GetAddCommand(post, entry);
                }
                if (post.Replace?.Count() > 0)
                {
                    command = GetReplaceCommand(post, entry);
                }
                else if (post.Remove?.Count() > 0)
                {
                    command = GetRemoveCommand(post, entry);
                }
            }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (command != null)
                {
                    entry = command.Execute();

                    _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });
                }

                response = new Nancy.Responses.TextResponse() { StatusCode = HttpStatusCode.Created };

                response.Headers.Add("Location", this.Context.EntryUrl(entry));
                return response;
            }
        }

        private BaseCommand<Entry> GetRemoveCommand(MicropubPayload post, Entry entry)
        {
            var removeCommand = new UpdateEntryAsRemoveCommand(_entryRepository)
            {
                Entry = entry,
                Name = post.Remove.Contains("name"),
                Content = post.Remove.Contains("content"),
                Categories = post.Remove.Contains("category"),
                Media = post.Remove.Contains("photo"),
                BookmarkOf = post.Remove.Contains("bookmark-of")
            };
            return removeCommand;
        }

        private BaseCommand<Entry> GetReplaceCommand(MicropubPayload post, Entry entry)
        {
            var replaceCommand = new UpdateEntryAsReplaceCommand(_entryRepository)
            {
                Entry = entry,
                Name = post.Replace.GetValueOrDefault("name")?[0]?.ToString(),
                Content = post.Replace.GetValueOrDefault("content")?[0]?.ToString(),
                Categories = post.Replace.GetValueOrDefault("category") as string[],
                Media = ParseMediaReference(post.Replace.GetValueOrDefault("photo"), "photo"),
                BookmarkOf = post.Replace.GetValueOrDefault("bookmark-of")?[0]?.ToString(),
                Published = post.Replace.GetValueOrDefault("post-status")?[0]?.ToString() != "draft"
            };

            return replaceCommand;
        }

        private BaseCommand<Entry> GetAddCommand(MicropubPayload post, Entry entry)
        {
            var addCommand = new UpdateEntryAsAddCommand(_entryRepository)
            {
                Entry = entry,
                Name = post.Add.GetValueOrDefault("name")?[0]?.ToString(),
                Content = post.Add.GetValueOrDefault("content")?[0]?.ToString(),
                Categories = post.Add.GetValueOrDefault("category") as string[],
                Media = ParseMediaReference(post.Add.GetValueOrDefault("photo"), "photo"),
                BookmarkOf = post.Add.GetValueOrDefault("bookmark-of")?[0]?.ToString(),
                Published = post.Add.GetValueOrDefault("post-status")?[0]?.ToString() != "draft"
            };

            return addCommand;
        }
    }

}
