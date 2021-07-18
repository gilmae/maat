using System;
using System.Collections.Generic;
using Events;
using mptest.Events;

namespace mptest.Commands
{
    public class AddPropertiesToPost
    {
        public Dictionary<string, object[]> Add { get; set; }

        //public string[] Delete { get; set; }

        //public Dictionary<string, object[]> Replace { get; set; }

        public bool IsValid(Aggregate aggregate)
        {
            return true;
        }

        public Event GetEvent(int version)
        {
            return new PropertiesAddedToPost { Properties = Add };
        }
    }
}
