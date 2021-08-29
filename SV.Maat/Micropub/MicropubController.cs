using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        IPostsProjection _entries;
        IUserStore _userStore;

        public MicropubController(ILogger<MicropubController> logger,
            IEventStore<Media> mediaRepository,
            IFileStore fileStore,
            Pipeline pipeline,
            CommandHandler commandHandler,
            IPostsProjection entries,
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

            if (!post.Properties.ContainsKey("url"))
            {
                post.Properties["url"] = new[] { location.ToString() };
            } else
            {
                post.Properties["url"].Append(location.ToString());
            }

            ICommand command = new CreatePost { Type = post.Type, Properties = post.Properties, OwnerId = this.UserId().Value };
            _commandHandler.Handle<Post>(id, command);

            return Created(location, null);
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

            Guid? entryId = _entries.Get(model.Url)?.Id;

            if (entryId == null || entryId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL could not be parsed."
                });
            }

            _ = _commandHandler.Handle<Post>(entryId.Value, new ReplaceInPost
            {
                Properties = new Dictionary<string, object[]> {
                    {"deleted", new object[]{
                        DateTime.UtcNow
                        }
                    }
                }
            });

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

            Guid? entryId = _entries.Get(model.Url)?.Id;

            if (entryId == null || entryId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL could not be parsed."
                });
            }

            _ = _commandHandler.Handle<Post>(entryId.Value, new DeleteFromPost
            {
                Properties = new string[]{"deleted"}
            });

            return Ok();
        }

        private IActionResult Update(MicropubPublishModel model)
        {
            if (string.IsNullOrEmpty(model.Url))
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL was not provided"
                });
            }

            Guid? entryId = _entries.Get(model.Url)?.Id;

            if (entryId == null || entryId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "URL could not be parsed."
                });
            }

            try
            {
                if (model.Add?.Count() > 0)
                {
                    return HandleAddUpdate(model.Add, entryId.Value);
                }
                if (model.Replace?.Count() > 0)
                {
                    return HandleReplaceUpdate(model.Replace, entryId.Value);
                }
                if (model.Delete?.Count() > 0)
                {
                    return HandleRemoveUpdate(model.Delete, entryId.Value);
                }
            }
            catch
            {
                throw;
            }

            return Ok();
        }

        private ActionResult HandleRemoveUpdate(string[] values, Guid id)
        {
            var cmd = new DeleteFromPost() { Properties = values };

            if (!_commandHandler.Handle<Post>(id, cmd))
            {
                return BadRequest($"Could not execute {cmd.GetType().Name}");
            }


            Post entry = _entries.Get(id);
            return Created(entry.Data.Properties["url"]?.FirstOrDefault().ToString(), null);
        }

        private ActionResult HandleReplaceUpdate(Dictionary<string, object[]> values, Guid id)
        {
            var cmd = new ReplaceInPost() { Properties = values };

            if (!_commandHandler.Handle<Post>(id, cmd))
            {
                return BadRequest($"Could not execute {cmd.GetType().Name}");
            }


            Post entry = _entries.Get(id);
            return Created(entry.Data.Properties["url"]?.FirstOrDefault().ToString(), null);
        }

        private ActionResult HandleAddUpdate(Dictionary<string, object[]> values, Guid id)
        {
            var cmd = new AddToPost() { Properties = values };

            if (!_commandHandler.Handle<Post>(id, cmd))
                {
                    return BadRequest($"Could not execute {cmd.GetType().Name}");
                }
            

            Post entry = _entries.Get(id);
            return Created(entry.Data.Properties["url"]?.FirstOrDefault().ToString(), null);
        }
    }
}
