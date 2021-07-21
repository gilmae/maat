using System;
using System.Collections.Generic;
using Events;
using mf;
using StrangeVanilla.Blogging.Events.Posts.Events;

namespace SV.Maat.Commands
{
    public class CreatePost : ICommand
    {
        public string[] Type { get; set; }
        public Dictionary<string, object[]> Properties { get; set; }
        public Microformat[] Children { get; set; }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate != null;
        }

        public Event GetEvent(int version)
        {
            return new PostCreated { Version = version, Type = Type, Properties = Properties, Children = Children };
        }
    }
}
