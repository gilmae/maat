using System;
using System.IO;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.lib;

namespace StrangeVanilla.Maat.Commands
{
    public class ProcessMediaUpload : BaseCommand<Media>
    {
        IEventStore<Media> _mediaStore;
        IFileStore _fileStore;

        public string Name { get; set; }
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
        public Stream Stream { get; set; }

        public ProcessMediaUpload(IEventStore<Media> mediaStore, IFileStore fileStore)
        {
            _mediaStore = mediaStore;
            _fileStore = fileStore;
        }

        public override Media Execute()
        {
            if (Stream != null)
            {
                Data = ReadStream(Stream);
            }
            string savePath = _fileStore.Save(Data);
            // Put it in the MEdia STore

            var m = new Media();
            MediaUploaded e = new MediaUploaded
            {
                Name = Name,
                MediaStoreId = savePath,
                MimeType = MimeType
            };

            e.Apply(m);
            _mediaStore.StoreEvent(e);
            return m;
        }

        private byte[] ReadStream(Stream data)
        {
            var bytes = new byte[data.Length];

            var index = 0;
            while (index < data.Length)
            {
                data.Read(bytes, index, 1000);
                index += 1000;
            }

            return bytes;
        }
    }
}
