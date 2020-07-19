﻿using System;
using System.Collections.Generic;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class AddContent : ICommand
    {
        public string[] Name { get; set; }
        public KeyValuePair<ContentType, string>[] Content { get; set; }
        public string BookmarkOf { get; set; }
        
        public bool IsValid(Aggregate aggregate)
        {
            return true;
        }

        public Event GetEvent(int version)
        {
            return new ContentSet()
            {
                Body = Content,
                Title = Name,
                BookmarkOf = BookmarkOf,
                Version = version
            };
        }
    }
}