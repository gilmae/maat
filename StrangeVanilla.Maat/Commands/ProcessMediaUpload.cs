using System;
using System.IO;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.lib;

namespace StrangeVanilla.Maat.Commands
{
    public class ProcessMediaUpload
    {
        IEventStore<Media> _mediaStore;
        IFileStore _fileStore;

        public ProcessMediaUpload(IEventStore<Media> mediaStore, IFileStore fileStore)
        {
            _mediaStore = mediaStore;
            _fileStore = fileStore;
        }

        public Media Execute(string name, string mimetype, byte[] data)
        {
            string savePath = _fileStore.Save(data);
            // Put it in the MEdia STore

            var m = new Media();
            MediaUploaded e = new MediaUploaded
            {
                Name = name,
                MediaStoreId = savePath,
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
