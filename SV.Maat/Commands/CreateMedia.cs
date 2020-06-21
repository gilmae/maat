using System;
using Events;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Commands
{
    public class CreateMedia : ICommand
    {
        public CreateMedia()
        {
        }

        public string Name { get; set; }
        public string SavePath { get; set; }
        public string MimeType { get; set; }

        public Event GetEvent(int version)
        {
            return new MediaUploaded
            {
                Name = Name,
                MediaStoreId = SavePath,
                MimeType = MimeType
            };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate.Version == 0;
        }
    }
}
