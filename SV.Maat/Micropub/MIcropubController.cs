using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
using static StrangeVanilla.Blogging.Events.Entry;
using System.Reflection;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public class MicropubController : ControllerBase
    {

        private readonly ILogger<MicropubController> _logger;
        IEventStore<Entry> _entryRepository;
        IMessageBus<Entry> _entryBus;
        IEventStore<Media> _mediaRepository;
        IFileStore _fileStore;
        IProjection<Entry> _entryView;

        public MicropubController(ILogger<MicropubController> logger,
            IEventStore<Entry> entryRepository,
            IEventStore<Media> mediaRepository,
            IMessageBus<Entry> entryBus,
            IFileStore fileStore,
            IProjection<Entry> entryView)
        {
            _logger = logger;
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;
            _entryBus = entryBus;
            _fileStore = fileStore;
            _entryView = entryView;
        }

        
        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Publish([FromBody]MicropubPublishModel post)
        {
            if (post.IsCreate()){
                return Create(post);
            }

            return Update(post);
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
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

                media = new List<Entry.MediaLink> { new Entry.MediaLink { Url = SV.Maat.lib.UrlHelper.MediaUrl(HttpContext, photo), Type = photo.Name } };
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

            string location = SV.Maat.lib.UrlHelper.EntryUrl(HttpContext, entry);
            //Response.Headers.Add("Location", UrlHelper.EntryUrl(HttpContext, entry));
            return base.Created(location, null);
        }

        [HttpGet]
        [Route("media/{id}")]
        public IActionResult GetMedia(Guid id)
        {
            var events = _mediaRepository.Retrieve(id);
            return Ok();

        }

        [HttpPost]
        [Route("media")]
        public IActionResult CreateMedia(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "No files provided"
                });
            }
            else
            {
                byte[] fileData = new byte[file.Length];
                using (var stream = new MemoryStream(fileData))
                {
                    file.CopyToAsync(stream);
                }
                var media = new ProcessMediaUpload(_mediaRepository, _fileStore)
                {
                    Name = file.Name,
                    MimeType = file.ContentType,
                    Data = fileData
                }.Execute();

                return Created(HttpContext.MediaUrl(media), null);
            }

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

            string location = SV.Maat.lib.UrlHelper.EntryUrl(HttpContext, entry);
            //Response.Headers.Add("Location", UrlHelper.EntryUrl(HttpContext, entry));
            return base.Created(location, null);
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


        [HttpGet]
        public IActionResult Query()
        {
            QueryType q = (QueryType)Enum.Parse(typeof(QueryType), Request.Query["q"]);
            if (q == QueryType.config)
            {
                return Ok(GetConfig());
            }
            else if (q == QueryType.source)
            {
                return GetSourceQuery(Request.Query["url"], Request.Query["properties[]"]);
            }
            return Ok();
        }





        private Config GetConfig()
        {
            return new Config { MediaEndpoint = Url.ActionLink("CreateMedia", "Micropub") };
        }

        private IActionResult GetSourceQuery(string url, string[] properties)
        {
            bool includeType = false;
            bool includeUrl = true;
            if (properties == null || properties.Count() == 0)
            {
                includeType = true;
            }

            var entries = _entryView.Get();
            
            if (!string.IsNullOrEmpty(url))
            {
                includeUrl = false;
                Guid entryId = HttpContext.GetEntryIdFromUrl(url);

                if (entryId == Guid.Empty)
                {
                    entries = entries.Where(e => (e.Syndications?.Contains(url)).GetValueOrDefault());
                }
                else
                {
                    entries = entries.Where(e => e.Id == entryId);
                }
            }

            var pagedEntries = entries.OrderByDescending(i => i.PublishedAt).Take(20);
            EntryToMicropubConverter converter = new EntryToMicropubConverter(properties);

            var micropubEntries = pagedEntries.Select(e => MicropubEnricher(e, converter.ToDictionary(e), includeUrl, includeType));

            return Ok(micropubEntries);
        }

        private dynamic MicropubEnricher(Entry entry, Dictionary<string, object> properties, bool includeUrl, bool includeType)
        {
            Dictionary<string, object> enrichedItem = new Dictionary<string, object>();
            enrichedItem["properties"] = properties;
            if (includeUrl)
            {
                enrichedItem["url"] = new[] { HttpContext.EntryUrl(entry) };
            }
            if (includeType)
            {
                enrichedItem["type"] = "h-entry";
            }
            return enrichedItem;
        }



        private class EntryToMicropubConverter
        {
            private Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
            private static Dictionary<string, string> columnMapper = new Dictionary<string, string>
            {
                {"name", "Title"},
                {"content", "Body" },
                {"category", "Categories" },
                {"photo", "AssociatedMedia" },
                {"in-reply-to", "ReplyTo" },
                {"post-status", "PublishedAt" },
                {"published", "PublishedAt" },
                {"bookmark-of", "BookmarkOf" }
            };

            public EntryToMicropubConverter() : this(null) { }
            public EntryToMicropubConverter(IList<string> columnsRequired)
            {
                if (columnsRequired == null || columnsRequired.Count == 0)
                {
                    columnsRequired = columnMapper.Keys.ToList();
                }

                columnsRequired = columnsRequired.Where(c => columnMapper.ContainsKey(c)).ToList();

                Type entry = typeof(Entry);
                foreach (string column in columnsRequired)
                {
                    var property = entry.GetProperty(columnMapper[column]);
                    if (property != null)
                    {
                        _properties[column] = property;
                    }
                }
            }

            public Dictionary<string, object> ToDictionary(Entry entry)
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (string key in _properties.Keys)
                {
                    values[key] = GetValue(key, entry);
                }

                return values;

            }

            public object GetValue(string columnName, Entry entry)
            {
                object value = _properties[columnName].GetValue(entry);
                switch (columnName)
                {
                    case "name":
                    case "content":
                    case "in-reply-to":
                    case "bookmark-of":
                    case "category":
                    case "published":
                        return value;
                    case "photo":
                        var media = value as IList<MediaLink>;
                        if (media != null)
                        {
                            return media.Where(x => x.Type == "photo");
                        }
                        return null;
                    case "post-status":
                        return value == null ? "draft" : "published";
                    default:
                        return null;

                }



            }
        }
    }


}
