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
using SV.Maat.Projections;
using Users;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public partial class MicropubController : ControllerBase
    {

        private readonly ILogger<MicropubController> _logger;
        IEventStore<Media> _mediaRepository;
        IFileStore _fileStore;
        Pipeline _pipeline;
        CommandHandler _commandHandler;
        IEntryProjection _entries;
        IUserStore _userStore;

        public MicropubController(ILogger<MicropubController> logger,
            IEventStore<Media> mediaRepository,
            IFileStore fileStore,
            Pipeline pipeline,
            CommandHandler commandHandler,
            IEntryProjection entries,
            IUserStore userStore
            )
        {
            _logger = logger;
            _mediaRepository = mediaRepository;
            _fileStore = fileStore;
            _pipeline = pipeline;
            _commandHandler = commandHandler;
            _entries = entries;
            _userStore = userStore;
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
            //else if (post.Action == ActionType.update.ToString())
            //{
            //    return Update(post);
            //}
            //else if (post.Action == ActionType.delete.ToString())
            //{
            //    return Delete(post);
            //}
            //else if (post.Action == ActionType.undelete.ToString())
            //{
            //    return Undelete(post);
            //}

            return BadRequest();
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult CreateFromForm([FromForm] MicropubFormCreateModel post)
        {
            MicropubPublishModel model = new MicropubPublishModel
            {
                Type = Request.Form["h"],
                Properties = new Dictionary<string, dynamic[]>()
            };

            foreach (var f in Request.Form.Files)
            {
                (Guid id, bool success) = HandleMediaUpload(f);
                if (success)
                {
                    string mediaUrl = this.Url.ActionLink("GetMediaFile", "Media", new { id });
                    if (!model.Properties.ContainsKey(f.Name))
                    {
                        model.Properties[f.Name] = new[] {  mediaUrl};
                    }
                    else
                    {
                        model.Properties[f.Name].Append(mediaUrl);
                    }
                }
            }

            foreach (var f in Request.Form){
                if (f.Key != "h" && !f.Key.StartsWith("mp-") && f.Key != "access_token" && f.Key!= "access-token"){
                    string name = f.Key.Replace("[]", "");
                    model.Properties[name] = f.Value;
                }
            }

            return Create(model);
        }

        private IActionResult Create(MicropubPublishModel post)
        {
            Guid id = Guid.NewGuid();
            Uri location = new Uri(this.Url.ActionLink("Entry", "Entry", new { id }));

            if (!post.Properties.ContainsKey("slug"))
            {
                post.Properties["slug"] = new[] { location.AbsolutePath.ToString() };
            } else
            {
                post.Properties["slug"].Append(location.AbsolutePath.ToString());
            }

            ICommand command = new CreatePost { Type = post.Type, Properties = post.Properties, OwnerId = this.UserId().Value };
            _commandHandler.Handle<Post>(id, command);

            return Created(location, null);
        }

        //public IActionResult Delete(MicropubPublishModel model)
        //{
        //    if (string.IsNullOrEmpty(model.Url))
        //    {
        //        return BadRequest(new
        //        {
        //            error = "invalid_request",
        //            error_description = "URL was not provided"
        //        });
        //    }

        //    Guid? entryId = _entries.Get(new Uri(model.Url)?.AbsolutePath)?.Id;

        //    if (entryId == null || entryId == Guid.Empty)
        //    {
        //        return BadRequest(new
        //        {
        //            error = "invalid_request",
        //            error_description = "URL could not be parsed."
        //        });
        //    }

        //    _commandHandler.Handle<Entry>(entryId.Value, new DeleteEntry());

        //    return Ok();
        //}

        //public IActionResult Undelete(MicropubPublishModel model)
        //{
        //    if (string.IsNullOrEmpty(model.Url))
        //    {
        //        return BadRequest(new
        //        {
        //            error = "invalid_request",
        //            error_description = "URL was not provided"
        //        });
        //    }

        //    Guid? entryId = _entries.Get(new Uri(model.Url)?.AbsolutePath)?.Id;

        //    if (entryId==null || entryId == Guid.Empty)
        //    {
        //        return BadRequest(new
        //        {
        //            error = "invalid_request",
        //            error_description = "URL could not be parsed."
        //        });
        //    }

        //    _commandHandler.Handle<Entry>(entryId.Value, new UndeleteEntry());

        //    return Ok();
        //}

        //private IActionResult Update(MicropubPublishModel model)
        //{
        //    if (string.IsNullOrEmpty(model.Url))
        //    {
        //        return BadRequest(new {
        //            error = "invalid_request",
        //            error_description = "URL was not provided"
        //        });
        //    }

        //    Guid? entryId = _entries.Get(new Uri(model.Url)?.AbsolutePath)?.Id;

        //    if (entryId == null || entryId == Guid.Empty)
        //    {
        //        return BadRequest(new
        //        {
        //            error = "invalid_request",
        //            error_description = "URL could not be parsed."
        //        });
        //    }

        //    try
        //    {
        //        if (model.Add?.Count() > 0) {
        //            return HandleAddUpdate(model.Add, entryId.Value);
        //        }
        //        else if (model.Replace?.Count() > 0) {
        //            return HandleReplaceUpdate(model.Replace, entryId.Value);
        //        }
        //        else if (model.Delete?.Count() > 0) { 
        //            return HandleRemoveUpdate(model.Delete, entryId.Value);
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return Ok();
        //}

        //private IEnumerable<Entry.MediaLink> ParseMediaReference(IEnumerable<dynamic> items, string type)
        //{
        //    IList<Entry.MediaLink> media = new List<Entry.MediaLink>();
        //    if (items == null)
        //    {
        //        return media;
        //    }

        //    foreach (dynamic item in items)
        //    {
        //        if (item is string)
        //        {
        //            media.Add(new Entry.MediaLink { Url = item.ToString(), Type = type });
        //        }
        //        else
        //        {
        //            var m = new Entry.MediaLink { Url = item.value, Type = type };
        //            try
        //            {
        //                m.Description = item.alt;
        //            }
        //            catch (RuntimeBinderException)
        //            {

        //            }
        //            media.Add(m);
        //        }
        //    }
        //    return media;
        //}

        //private ActionResult HandleRemoveUpdate(string[] values, Guid id)
        //{
        //    List<ICommand> commands = new List<ICommand> { };
        //    if (values.Contains("name") || values.Contains("content") || values.Contains("bookmark-of"))
        //    {
        //        commands.Add(new SetContent
        //        {
        //            Name = values.Contains("name") ? new Content() : null,
        //            Content = values.Contains("content") ? new Content() : null,
        //            BookmarkOf = values.Contains("bookmark-of") ? "" : null
        //        });
        //    }

        //    if (values.Contains("reply-to"))
        //    {
        //        commands.Add(new ReplyTo { ReplyToUrl = string.Empty });
        //    }

        //    if (values.Contains("category"))
        //    {
        //        commands.Add(new ClearCategoriesFromEntry());
        //    }

        //    foreach (ICommand command in commands)
        //    {
        //        if (!_commandHandler.Handle<Entry>(id, command))
        //        {
        //            return BadRequest($"Could not {command.GetType().Name}");
        //        }
        //    }

        //    Entry entry = _entries.Get(id);
        //    return Created(UrlHelper.EntryUrl(entry, _userStore.Find(entry.OwnerId)), null);
        //}

        //private ActionResult HandleReplaceUpdate(Dictionary<string, string[]> values, Guid id)
        //{
        //    List<ICommand> commands = new List<ICommand> { };
        //    commands.Add(new SetContent
        //    {
        //        Name = ContentHelper.ParseContentArray(values.GetValueOrDefault("name")),
        //        Content = ContentHelper.ParseContentArray(values.GetValueOrDefault("content")),
        //        BookmarkOf = values.GetValueOrDefault("bookmark-of")?[0]?.ToString()
        //    });

        //    if (values.GetValueOrDefault("category") != null)
        //    {
        //        commands.Add(new ClearCategoriesFromEntry());
        //        string[] categories = (values.GetValueOrDefault("category") as object[]).Select(x => x.ToString()).ToArray();
        //        commands.AddRange(categories.Select(c => new AddToCategory { Category = c }));
        //    }

        //    var media = ParseMediaReference(values.GetValueOrDefault("photo"), "photo");
        //    if (media.Any())
        //    {
        //        commands.Add(new ClearMediaFromEntry());
        //        commands.AddRange(media.Select(m => new AttachMediaToEntry { Description = m.Description, Type = m.Type, Url = m.Url }));
        //    }

        //    //if (post.Properties.GetValueOrDefault("mp-syndicate-to") != null)
        //    //{
        //    //    string[] syndicateTo = (post.Properties.GetValueOrDefault("mp-syndicate-to") as object[]).Select(x => x.ToString()).ToArray();
        //    //    commands.AddRange(syndicateTo.Select(c => new Syndicate { SyndicationAccount = c }));
        //    //}

        //    //string postStatus = post.Properties.GetValueOrDefault("post-status")?[0]?.ToString();
        //    //if (postStatus == null || postStatus != "draft")
        //    //{
        //    //    commands.Add(new PublishEntry());
        //    //}

        //    foreach (ICommand command in commands)
        //    {
        //        if (!_commandHandler.Handle<Entry>(id, command))
        //        {
        //            return BadRequest($"Could not {command.GetType().Name}");
        //        }
        //    }

        //    Entry entry = _entries.Get(id);
        //    return Created(UrlHelper.EntryUrl(entry, _userStore.Find(entry.OwnerId)), null);
        //}

        //private ActionResult HandleAddUpdate(Dictionary<string, string[]> values, Guid id)
        //{
        //    List<ICommand> commands = new List<ICommand> { };
        //    commands.Add(new AddContent
        //    {
        //        Name = ContentHelper.ParseContentArray(values.GetValueOrDefault("name")),
        //        Content = ContentHelper.ParseContentArray(values.GetValueOrDefault("content")),
        //        BookmarkOf = values.GetValueOrDefault("bookmark-of")?[0]?.ToString()
        //    });

        //    if (values.GetValueOrDefault("category") != null)
        //    {
        //        string[] categories = (values.GetValueOrDefault("category") as object[]).Select(x => x.ToString()).ToArray();
        //        commands.AddRange(categories.Select(c => new AddToCategory { Category = c }));
        //    }

        //    var media = ParseMediaReference(values.GetValueOrDefault("photo"), "photo");
        //    commands.AddRange(media.Select(m => new AttachMediaToEntry { Description = m.Description, Type = m.Type, Url = m.Url }));

        //    if (values.GetValueOrDefault("mp-syndicate-to") != null)
        //    {
        //        string[] syndicateTo = (values.GetValueOrDefault("mp-syndicate-to") as object[]).Select(x => x.ToString()).ToArray();
        //        commands.AddRange(syndicateTo.Select(c => new Syndicate { SyndicationAccount=c}));
        //    }

        //    foreach (ICommand command in commands)
        //    {
        //        if (!_commandHandler.Handle<Entry>(id, command))
        //        {
        //            return BadRequest($"Could not {command.GetType().Name}");
        //        }
        //    }

        //    Entry entry = _entries.Get(id);
        //    return Created(UrlHelper.EntryUrl(entry, _userStore.Find(entry.OwnerId)), null);
        //}

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
