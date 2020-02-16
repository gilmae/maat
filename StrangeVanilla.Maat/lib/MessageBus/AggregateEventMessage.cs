using System;
namespace StrangeVanilla.Maat.lib.MessageBus
{
    public class AggregateEventMessage
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
