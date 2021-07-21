using System;
using System.Collections.Generic;
using Events;
using StrangeVanilla.Blogging.Events.Posts.Events;

namespace SV.Maat.Commands
{
    public class RemovePropertiesFromPost : ICommand
    {
        public string[] Delete { get; set; }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate != null;
        }

        public Event GetEvent(int version)
        {
            return new PropertiesRemovedFromPost { Version = version, Properties = Delete };
        }
    }
}
