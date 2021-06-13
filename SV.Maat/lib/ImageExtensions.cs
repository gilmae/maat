using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace SV.Maat.lib
{
    public static class ImageExtensions
    {
        public static Image ScaleToWidth(this Image image, int width, IImageEncoder encoder)
        {
            float scale = (float)width / image.Width;
            int height = (int)Math.Floor(image.Height * scale);

            image.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));
            var ms = new MemoryStream();
            image.Save(ms, encoder);
            ms.Position = 0;

            byte[] resizedData = new byte[ms.Length];
            for (int i = 0; i < ms.Length; i++)
            {
                ms.Read(resizedData, i, 1);
            }

            ms.Close();
            ms.Dispose();

            return Image.Load(resizedData);
        }

        public static byte[] ToBytes(this Image image)
        {
            return Convert.FromBase64String(image.ToBase64String());
        }
    }
}
