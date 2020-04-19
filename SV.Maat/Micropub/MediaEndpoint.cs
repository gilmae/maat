﻿using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                var media = new ProcessMediaUpload(_mediaRepository, _fileStore)
                {
                    Name = file.Name,
                    MimeType = file.ContentType,
                    Data = fileData
                }.Execute();

                return Created(HttpContext.MediaUrl(media), null);
            }

        }
    }
}
