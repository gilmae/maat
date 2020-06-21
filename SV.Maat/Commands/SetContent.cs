using System;
using Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class SetContent : ICommand
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string BookmarkOf { get; set; }
        
        public bool IsValid(Aggregate aggregate)
        {
            throw new NotImplementedException();
        }

        public Event GetEvent(int version)
        {
            return new EntryUpdated()
            {
                Body = Content,
                Title = Name,
                BookmarkOf = BookmarkOf,
                Version = version
            };
        }
    }
}
