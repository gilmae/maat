using System;
using System.Collections.Generic;
using Events;
using StrangeVanilla.Blogging.Events.Posts.Events;

namespace SV.Maat.Commands
{
    public class ReplacePropertiesOnPost : ICommand
    {
        public Dictionary<string, object[]> Replace { get; set; }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate != null;
        }

        public Event GetEvent(int version)
        {
            return new PropertiesReplacedOnPost { Version = version, Properties = Replace };
        }
    }
}
