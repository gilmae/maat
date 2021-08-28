using System;
using System.Collections.Generic;
using Events;
using mf;
using StrangeVanilla.Blogging.Events.Posts.Events;

namespace SV.Maat.Commands
{
    public class AddToPost : ICommand
    {
        public Dictionary<string, object[]> Properties { get; set; }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate != null;
        }

        public Event GetEvent(int version)
        {
            return new PropertiesAddedToPost { Version = version, Properties = Properties };
        }
    }
}
