using System;
using System.IO;
using Events;
using StrangeVanilla.Blogging.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class ProcessMediaUpload
    {
        IEventStore<Media> _mediaStore;

        public ProcessMediaUpload(IEventStore<Media> mediaStore)
        {
            _mediaStore = mediaStore;
        }

        public Media Execute(string name, string mimetype, byte[] data)
        {
            string savePath = Path.GetFullPath(".");
            // Put it in the MEdia STore
            //using (var reader = new StreamReader(file.Value))
            //{
            //    var bytes = reader.ReadToEnd();
            //    File.WriteAllBytes(savePath, bytes);

            //}
            var m = new Media();
            MediaUploaded e = new MediaUploaded
            {
                Name = name,
                MediaStoreId = string.Format("{1}/{0}", Guid.NewGuid(), savePath),
                MimeType = mimetype
            };
            e.Apply(m);
            _mediaStore.StoreEvent(e);
            return m;
        }

        public Media Execute(string name, string mimetype, Stream data)
        {
            var bytes = new byte[data.Length];

            var index = 0;
            while (index < data.Length)
            {
                data.Read(bytes, index, 1000);
                index += 1000;
            }

            return Execute(name, mimetype, bytes);
        }
    }
}
