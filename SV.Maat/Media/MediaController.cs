using System;
using Events;
using StrangeVanilla.Blogging.Events;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.lib.FileStore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SV.Maat.lib;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace SV.Maat.MediaView
{
    [Route("media")]
    public class MediaController : Controller
    {
        readonly IProjection<Media> _mediaProjection;
        readonly IFileStore _fileStore;
        IMemoryCache _memoryCache;
        readonly int[] sizes = new int[] { 16, 32, 64, 128, 640, 800, 1024, 2048, 4096 };

        public MediaController(IProjection<Media> mediaProjection, IFileStore fileStore, IMemoryCache memoryCache)
        {
            _mediaProjection = mediaProjection;
            _fileStore = fileStore;
            _memoryCache = memoryCache;
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
                // Width will be considered the maximum width desired. Maat will only support a set of defined widths,
                // so find the width we can return based less than or equal to the max width

                int actualWidth = sizes.LastOrDefault(x => x <= width);
                if (actualWidth == 0)
                {
                    actualWidth = sizes.First();
                }

                string cacheKey = $"{id}_{actualWidth}";

                if (_memoryCache.TryGetValue<byte[]>(cacheKey, out byte[] data))
                {
                    return new FileContentResult(data, m.MimeType);
                }

                data = _fileStore.Get(m.MediaStoreId);

                using (Image image = Image.Load(data))
                {
                    float scale = (float)actualWidth / image.Width;
                    int height = (int)Math.Floor(image.Height * scale);
                    SixLabors.ImageSharp.Formats.IImageEncoder encoder = new JpegEncoder();
                    if (m.MimeType == "image/gif")
                    {
                        encoder = new GifEncoder();
                    }
                    else if (m.MimeType == "image/png")
                    {
                        encoder = new PngEncoder();
                    }

                    image.Mutate(x => x.Resize(actualWidth, height, KnownResamplers.Lanczos3));
                    var ms = new MemoryStream();
                    image.Save(ms, encoder);
                    ms.Position = 0;

                    byte[] resizedData = new byte[ms.Length];
                    for (int i=0; i < ms.Length; i++)
                    {
                        ms.Read(resizedData, i, 1);
                    }

                    ms.Close();
                    ms.Dispose();

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));


                    _memoryCache.Set<byte[]>(cacheKey, resizedData, cacheEntryOptions);
                    return new FileContentResult(resizedData, m.MimeType);
                }
            }

            return NotFound();
        }


    }
}
