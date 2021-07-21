using System;
using System.Collections.Generic;
using Events;
using StrangeVanilla.Blogging.Events.Posts.Events;

namespace SV.Maat.Commands
{
    public class AddPropertiesToPost : ICommand
    {
        public Dictionary<string, object[]> Add { get; set; }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate != null;
        }

        public Event GetEvent(int version)
        {
            return new PropertiesAddedToPost { Version = version, Properties = Add };
        }
    }
}
