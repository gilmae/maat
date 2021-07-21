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
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
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
                (Guid id, bool success) = HandleMediaUpload(file);
                if (success)
                {
                    return Created(this.Url.ActionLink("GetMediaFile", "Media", new { id }), null);
                }
                else
                {
                    return BadRequest("Could not upload file");
                }
            }

        }

        public (Guid, bool) HandleMediaUpload(IFormFile file)
        {
            Guid id = Guid.Empty;

            byte[] fileData = new byte[file.Length];
            using (var stream = new MemoryStream(fileData))
            {
                file.CopyTo(stream);
            }
            string filePath = _fileStore.Save(fileData);
            id = Guid.NewGuid();
            if (!_commandHandler.Handle<Media>(id,
                new CreateMedia
                {
                    Name = file.Name,
                    MimeType = file.ContentType,
                    SavePath = filePath
                }))
            {
                return (id, false);
            }

            return (id, true);
        }
    }
}
