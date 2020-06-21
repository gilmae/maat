using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StrangeVanilla.Blogging.Events;
using SV.Maat.Commands;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.lib;

namespace SV.Maat.Micropub
{
    public partial class MicropubController
    {
        [HttpPost]
        [Route("media")]
        [Authorize(AuthenticationSchemes =IndieAuthTokenHandler.SchemeName)]
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
                string filePath = _fileStore.Save(fileData);
                Guid id = Guid.NewGuid();
                if (!_commandHandler.Handle<Media>(id,
                    new CreateMedia
                    {
                        Name = file.Name,
                        MimeType = file.ContentType,
                        SavePath = filePath
                    }))
                {
                    return BadRequest("Could not upload file");
                }

                //return Created(HttpContext.MediaUrl(media), null);
                return Created(this.Url.ActionLink("GetMediaFile", "Media", new { id }), null);
            }

        }
    }
}
