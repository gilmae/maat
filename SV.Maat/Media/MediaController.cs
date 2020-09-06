using System;
using Events;
using StrangeVanilla.Blogging.Events;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.lib.FileStore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace SV.Maat.MediaView
{
    [Route("media")]
    public class MediaController : Controller
    {
        readonly IProjection<Media> _mediaProjection;
        readonly IFileStore _fileStore;

        public MediaController(IProjection<Media> mediaProjection, IFileStore fileStore)
        {
            _mediaProjection = mediaProjection;
            _fileStore = fileStore;
        }

        [Route("{id}")]
        [HttpGet]
        public IActionResult GetMediaFile(Guid id)
        {
            var m = _mediaProjection.Get(id);
            if (m != null)
            {
                byte[] data = _fileStore.Get(m.MediaStoreId);

                return new FileContentResult(data, m.MimeType);
            }

            return NotFound();
        }

        [Route("{id}/{width}")]
        [HttpGet]
        public IActionResult GetSizedMediaFile(Guid id, int width)
        {
            var m = _mediaProjection.Get(id);
            if (m != null)
            {
                byte[] data = _fileStore.Get(m.MediaStoreId);

                using (Image image = Image.Load(data))
                {
                    float scale = (float)width / image.Width;
                    int height = (int)Math.Floor(image.Height * scale);

                    image.Mutate(x => x.Resize(width,height));
                    var ms = new MemoryStream();
                        image.SaveAsJpeg(ms);
                        ms.Position = 0;
                        return new FileStreamResult(ms, Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/jpeg"));
                }
            }

            return NotFound();
        }


    }
}
