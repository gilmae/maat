using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using SV.Maat.Commands;
using SV.Maat.lib.FileStore;
using SV.Maat.lib.MessageBus;
using SV.Maat.Micropub.Models;
using SV.Maat.lib;
using Microsoft.AspNetCore.Authorization;
using SV.Maat.IndieAuth.Middleware;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public partial class MicropubController : ControllerBase
    {

        private readonly ILogger<MicropubController> _logger;
        IEventStore<Entry> _entryRepository;
        IMessageBus<Entry> _entryBus;
        IEventStore<Media> _mediaRepository;
        IFileStore _fileStore;

        public MicropubController(ILogger<MicropubController> logger,
            IEventStore<Entry> entryRepository,
            IEventStore<Media> mediaRepository,
            IMessageBus<Entry> entryBus,
            IFileStore fileStore
            )
        {
            _logger = logger;
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            _fileStore = fileStore;
        }
        
        [HttpPost]
        [Consumes("application/json")]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult Publish([FromBody]MicropubPublishModel post)
        {
            
            if (post.IsCreate())
            {
                return Create(post);
            }
            else if (post.Action == ActionType.update.ToString())
            {
                return Update(post);
            }
            else if (post.Action == ActionType.delete.ToString())
            {
                return Delete(post);
            }

            return BadRequest();
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult CreateFromForm([FromForm]MicropubFormCreateModel post)
        {
            ProcessMediaUpload mediaProcessor = new ProcessMediaUpload(_mediaRepository, _fileStore);
            IEnumerable<Entry.MediaLink> media = new List<Entry.MediaLink>();
            if (post.Photo != null)
            {

                byte[] fileData = new byte[post.Photo.Length];
                using (var stream = new MemoryStream(fileData))
                {
                    post.Photo.CopyToAsync(stream);
                }
                mediaProcessor.Name = post.Photo.Name;
                mediaProcessor.MimeType = post.Photo.ContentType;
                mediaProcessor.Data = fileData;

                var photo = mediaProcessor.Execute();

                media = new List<Entry.MediaLink> { new Entry.MediaLink { Url = this.Url.MediaUrl(photo), Type = photo.Name } };
            }

            string postStatus = post.PostStatus;
            CreateEntryCommand command = new CreateEntryCommand(_entryRepository)
            {
                Content = post.Content,
                Name = post.Title,
                Categories = post.Categories,
                Media = media,
                BookmarkOf = post.BookmarkOf,
                Published = postStatus == null || postStatus != "draft",
                ReplyTo = post.ReplyTo,
            };

            var entry = command.Execute();

            _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });

            string location = UrlHelper.EntryUrl(HttpContext, entry);
            return base.Created(location, null);
        }

        

        private IActionResult Create(MicropubPublishModel post)
        {
            var photos = ParseMediaReference(post.Properties.GetValueOrDefault("photo"), "photo");

            string inReplyTo = post.Properties.GetValueOrDefault("in-reply-to")?[0]?.ToString();
            string postStatus = post.Properties.GetValueOrDefault("post-status")?[0]?.ToString();
            CreateEntryCommand command = new CreateEntryCommand(_entryRepository)
            {
                Content = post.Properties.GetValueOrDefault("content")?[0]?.ToString(),
                Name = post.Properties.GetValueOrDefault("name")?[0]?.ToString(),
                Categories = (post.Properties.GetValueOrDefault("category") as object[]).Select(x => x.ToString()).ToArray(),
                Media = photos,
                BookmarkOf = post.Properties.GetValueOrDefault("bookmark-of")?[0]?.ToString(),
                Published = postStatus == null || postStatus != "draft",
                ReplyTo = inReplyTo,
            };

            var entry = command.Execute();

            _entryBus.Publish(new AggregateEventMessage { Id = entry.Id, Version = entry.Version });

            string location = UrlHelper.EntryUrl(HttpContext, entry);
            return base.Created(location, null);
        }

        public IActionResult Delete(MicropubPublishModel model)
        {
            if (string.IsNullOrEmpty(model.Url))
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL was not provided"
                });
            }

            Guid entryId = HttpContext.GetEntryIdFromUrl(model.Url);

            if (entryId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL could not be parsed."
                });
            }

            var entry = new Entry(entryId);

            var command = new DeleteEntry(_entryRepository) { Entry = entry };
            command.Execute();

            return Ok();
        }

        private IActionResult Update(MicropubPublishModel model)
        {
            if (string.IsNullOrEmpty(model.Url))
            {
                return BadRequest(new {
                    error = "invalid_request",
                    error_description = "URL was not provided"
                });
            }

            Guid entryId = HttpContext.GetEntryIdFromUrl(model.Url);

            if (entryId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL could not be parsed."
                });
            }

            var entry = new Entry(entryId);
            BaseCommand<Entry> command = null;
            try
            {
                if (model.Add?.Count() > 0) {
                    command = GetAddCommand(model, entry);
                }
                else if (model.Replace?.Count() > 0) { 
                        command = GetReplaceCommand(model, entry);
                }
                else if (model.Remove?.Count() > 0) { 
                        command = GetRemoveCommand(model, entry);
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

            return Created(HttpContext.EntryUrl(entry), null);
        }

        private IEnumerable<Entry.MediaLink> ParseMediaReference(IEnumerable<dynamic> items, string type)
        {
            IList<Entry.MediaLink> media = new List<Entry.MediaLink>();
            if (items == null)
            {
                return media;
            }

            foreach (dynamic item in items)
            {
                if (item is string)
                {
                    media.Add(new Entry.MediaLink { Url = item.ToString(), Type = type });
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

        private BaseCommand<Entry> GetRemoveCommand(MicropubPublishModel post, Entry entry)
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

        private BaseCommand<Entry> GetReplaceCommand(MicropubPublishModel post, Entry entry)
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

        private BaseCommand<Entry> GetAddCommand(MicropubPublishModel post, Entry entry)
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
