﻿using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryCategorised : Event<Entry>
    {
        public string Category { get; set; }

        public EntryCategorised() { }
        public EntryCategorised(Entry entry, string category)
        {
            AggregateId = entry.Id;
            Category = category;
        }

        public new void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            if (aggregate.Categories == null)
            {
                aggregate.Categories = new List<string> { Category };
            }
            else
            {
                aggregate.Categories.Add(Category);
            }
        }
    }
}
