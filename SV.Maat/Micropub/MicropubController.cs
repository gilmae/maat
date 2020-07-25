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
using SV.Maat.Micropub.Models;
using SV.Maat.lib;
using Microsoft.AspNetCore.Authorization;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.lib.Pipelines;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public partial class MicropubController : ControllerBase
    {

        private readonly ILogger<MicropubController> _logger;
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;
        IFileStore _fileStore;
        Pipeline _pipeline;
        CommandHandler _commandHandler;

        public MicropubController(ILogger<MicropubController> logger,
            IEventStore<Entry> entryRepository,
            IEventStore<Media> mediaRepository,
            IFileStore fileStore,
            Pipeline pipeline,
            CommandHandler commandHandler
            )
        {
            _logger = logger;
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _fileStore = fileStore;
            _pipeline = pipeline;
            _commandHandler = commandHandler;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        [Route("/entries/{id}")]
        public IActionResult Entry([FromRoute] Guid id)
        {
            return Ok();
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
            else if (post.Action == ActionType.undelete.ToString())
            {
                return Undelete(post);
            }

            return BadRequest();
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult CreateFromForm([FromForm] MicropubFormCreateModel post)
        {
            IEnumerable<Entry.MediaLink> media = new List<Entry.MediaLink>();
            if (post.Photo != null)
            {
                byte[] fileData = new byte[post.Photo.Length];
                using (var stream = new MemoryStream(fileData))
                {
                    post.Photo.CopyToAsync(stream);
                }
                string filePath = _fileStore.Save(fileData);
                Guid id = Guid.NewGuid();
                if (!_commandHandler.Handle<Media>(id,
                    new CreateMedia {
                        Name = post.Photo.Name,
                        MimeType = post.Photo.ContentType,
                        SavePath = filePath
                    }))
                {
                    return BadRequest("Could not upload file");
                }

                media = new List<Entry.MediaLink> { new Entry.MediaLink { Url = this.Url.ActionLink("GetMediaFile", "Media", new { id }), Type = post.Photo.Name } };
            }

            string postStatus = post.PostStatus;

            return HandleCreate(new[] { new KeyValuePair<ContentType, string>(ContentType.plaintext, post.Content) }, new []{ post.Title}, post.GetCategories(), media, post.BookmarkOf, post.ReplyTo, postStatus, post.SyndicateTo);
        }

        private IActionResult Create(MicropubPublishModel post)
        {
            var photos = ParseMediaReference(post.Properties.GetValueOrDefault("photo"), "photo");

            string inReplyTo = post.Properties.GetValueOrDefault("in-reply-to")?[0]?.ToString();
            string postStatus = post.Properties.GetValueOrDefault("post-status")?[0]?.ToString();
            string[] categories = new string[0];
            if (post.Properties.GetValueOrDefault("category") != null)
            {
                categories = (post.Properties.GetValueOrDefault("category") as object[]).Select(x => x.ToString()).ToArray();
            }

            string[] syndicateTo = new string[0];
            if (post.Properties.GetValueOrDefault("mp-syndicate-to") != null)
            {
                syndicateTo = (post.Properties.GetValueOrDefault("mp-syndicate-to") as object[]).Select(x => x.ToString()).ToArray();
            }

            KeyValuePair<ContentType, string>[] content = ContentHelper.ParseContentArray(post.Properties.GetValueOrDefault("content"));
            return HandleCreate(content,
                (post.Properties.GetValueOrDefault("name") as object[])?.Select(x => x.ToString()).ToArray(),
                categories,
                photos,
                post.Properties.GetValueOrDefault("bookmark-of")?[0]?.ToString(),
                inReplyTo,
                postStatus,
                syndicateTo
                );
        }

        private ActionResult HandleCreate(KeyValuePair<ContentType, string>[] content,
            string[] name,
            string[] categories,
            IEnumerable<Entry.MediaLink> media,
            string bookmark,
            string replyTo,
            string postStatus,
            string[] syndicateTo)
        {
            Guid id = Guid.NewGuid();
            List<ICommand> commands = new List<ICommand> { new CreateEntry() };
            commands.Add(new SetContent { Name = name, Content = content, BookmarkOf = bookmark });
            if (categories != null)
            {
                commands.AddRange(categories.Select(c => new AddToCategory { Category = c }));
            }
            if (media != null)
            {
                commands.AddRange(media.Select(m => new AttachMediaToEntry { Description = m.Description, Url = m.Url, Type = m.Type }));
            }
            if (!string.IsNullOrEmpty(replyTo))
            {
                commands.Add(new ReplyTo { ReplyToUrl = replyTo });
            }

            if (syndicateTo != null)
            {
                commands.AddRange(syndicateTo.Select(s => new Syndicate { SyndicationAccount = s }));
            }

            if (postStatus == null || postStatus != "draft")
            {
                commands.Add(new PublishEntry());
            }

            foreach (ICommand command in commands)
            {
                if (!_commandHandler.Handle<Entry>(id, command))
                {
                    return BadRequest($"Could not {command.GetType().Name}");
                }
            }

            string location = this.Url.ActionLink("Entry", "Micropub", new { id });
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

            _commandHandler.Handle<Entry>(entryId, new DeleteEntry());

            return Ok();
        }

        public IActionResult Undelete(MicropubPublishModel model)
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

            _commandHandler.Handle<Entry>(entryId, new UndeleteEntry());

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

            try
            {
                if (model.Add?.Count() > 0) {
                    return HandleAddUpdate(model.Add, entryId);
                }
                else if (model.Replace?.Count() > 0) {
                    return HandleReplaceUpdate(model.Replace, entryId);
                }
                else if (model.Delete?.Count() > 0) { 
                    return HandleRemoveUpdate(model.Delete, entryId);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Ok();
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

        private ActionResult HandleRemoveUpdate(string[] values, Guid id)
        {
            List<ICommand> commands = new List<ICommand> { };
            if (values.Contains("name") || values.Contains("content") || values.Contains("bookmark-of"))
            {
                commands.Add(new SetContent
                {
                    Name = values.Contains("name") ? new string[0] : null,
                    Content = values.Contains("content") ? new KeyValuePair<ContentType, string>[0] : null,
                    BookmarkOf = values.Contains("bookmark-of") ? "" : null
                });
            }

            if (values.Contains("reply-to"))
            {
                commands.Add(new ReplyTo { ReplyToUrl = string.Empty });
            }

            if (values.Contains("category"))
            {
                commands.Add(new ClearCategoriesFromEntry());
            }

            foreach (ICommand command in commands)
            {
                if (!_commandHandler.Handle<Entry>(id, command))
                {
                    return BadRequest($"Could not {command.GetType().Name}");
                }
            }

            return Created(this.Url.ActionLink("Entry", "Micropub", new { id }), null);
        }

        private ActionResult HandleReplaceUpdate(Dictionary<string, string[]> values, Guid id)
        {
            List<ICommand> commands = new List<ICommand> { };
            commands.Add(new SetContent
            {
                Name = values.GetValueOrDefault("name"),
                Content = ContentHelper.ParseContentArray(values.GetValueOrDefault("content")),
                BookmarkOf = values.GetValueOrDefault("bookmark-of")?[0]?.ToString()
            });

            if (values.GetValueOrDefault("category") != null)
            {
                commands.Add(new ClearCategoriesFromEntry());
                string[] categories = (values.GetValueOrDefault("category") as object[]).Select(x => x.ToString()).ToArray();
                commands.AddRange(categories.Select(c => new AddToCategory { Category = c }));
            }

            var media = ParseMediaReference(values.GetValueOrDefault("photo"), "photo");
            if (media.Any())
            {
                commands.Add(new ClearMediaFromEntry());
                commands.AddRange(media.Select(m => new AttachMediaToEntry { Description = m.Description, Type = m.Type, Url = m.Url }));
            }

            //if (post.Properties.GetValueOrDefault("mp-syndicate-to") != null)
            //{
            //    string[] syndicateTo = (post.Properties.GetValueOrDefault("mp-syndicate-to") as object[]).Select(x => x.ToString()).ToArray();
            //    commands.AddRange(syndicateTo.Select(c => new Syndicate { SyndicationAccount = c }));
            //}

            //string postStatus = post.Properties.GetValueOrDefault("post-status")?[0]?.ToString();
            //if (postStatus == null || postStatus != "draft")
            //{
            //    commands.Add(new PublishEntry());
            //}

            foreach (ICommand command in commands)
            {
                if (!_commandHandler.Handle<Entry>(id, command))
                {
                    return BadRequest($"Could not {command.GetType().Name}");
                }
            }

            return Created(this.Url.ActionLink("Entry", "Micropub", new { id }), null);
        }

        private ActionResult HandleAddUpdate(Dictionary<string, string[]> values, Guid id)
        {
            List<ICommand> commands = new List<ICommand> { };
            commands.Add(new AddContent
            {
                Name = values.GetValueOrDefault("name"),
                Content = ContentHelper.ParseContentArray(values.GetValueOrDefault("content")),
                BookmarkOf = values.GetValueOrDefault("bookmark-of")?[0]?.ToString()
            });

            if (values.GetValueOrDefault("category") != null)
            {
                string[] categories = (values.GetValueOrDefault("category") as object[]).Select(x => x.ToString()).ToArray();
                commands.AddRange(categories.Select(c => new AddToCategory { Category = c }));
            }

            var media = ParseMediaReference(values.GetValueOrDefault("photo"), "photo");
            commands.AddRange(media.Select(m => new AttachMediaToEntry { Description = m.Description, Type = m.Type, Url = m.Url }));

            if (values.GetValueOrDefault("mp-syndicate-to") != null)
            {
                string[] syndicateTo = (values.GetValueOrDefault("mp-syndicate-to") as object[]).Select(x => x.ToString()).ToArray();
                commands.AddRange(syndicateTo.Select(c => new Syndicate { SyndicationAccount=c}));
            }

            foreach (ICommand command in commands)
            {
                if (!_commandHandler.Handle<Entry>(id, command))
                {
                    return BadRequest($"Could not {command.GetType().Name}");
                }
            }

            return Created(this.Url.ActionLink("Entry", "Micropub", new { id }), null);
        }

        private byte[] ReadStream(Stream data)
        {
            var bytes = new byte[data.Length];

            var index = 0;
            while (index < data.Length)
            {
                data.Read(bytes, index, 1000);
                index += 1000;
            }

            return bytes;
        }
    }
}
