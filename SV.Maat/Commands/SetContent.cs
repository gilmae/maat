using System;
using System.Collections.Generic;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class SetContent : ICommand
    {
        public Content Name { get; set; }
        public Content Content { get; set; }
        public string BookmarkOf { get; set; }
        
        public bool IsValid(Aggregate aggregate)
        {
            return true;
        }

        public Event GetEvent(int version)
        {
            return new ContentSet2()
            {
                Body = Content,
                Title = Name,
                BookmarkOf = BookmarkOf,
                Version = version
            };
        }
    }
}
