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
                    Nancy.Response response;
                    if (this.Request.Files == null)
                    {
                        response = new Nancy.Responses.JsonResponse(new
                        {
                            error = "invalid_request",
                            error_description = "No files provided"
                        }, new Nancy.Serialization.JsonNet.JsonNetSerializer(), this.Context.Environment);
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    else if (this.Request.Files.Count() > 1)
                    {
                        response = new Nancy.Responses.JsonResponse(new
                        {
                            error = "invalid_request",
                            error_description = "Too many files provided."
                        }, new Nancy.Serialization.JsonNet.JsonNetSerializer(), this.Context.Environment);
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                    else
                    {
                        var f = Request.Files.First();
                        var media = new ProcessMediaUpload(_mediaRepository, fileStore)
                        {
                            Name = f.Name,
                            MimeType = f.ContentType,
                            Stream = f.Value
                        }.Execute();


                        response = new Nancy.Responses.TextResponse() { StatusCode = HttpStatusCode.Created };

                        response.Headers.Add("Location", UrlHelper.MediaUrl(Context, media));
                    }
                    return response;
                });
        }
    }
}
