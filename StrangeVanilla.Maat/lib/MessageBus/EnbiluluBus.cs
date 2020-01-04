using System;
using libEnbilulu;

namespace StrangeVanilla.Maat.lib.MessageBus
{
    public class EnbiluluBus<T> : IMessageBus<T> 
    {
        public Client client;
        public string streamName = typeof(T).Name;

        public EnbiluluBus()
        {
            client = new Client("http://localhost");//Environment.GetEnvironmentVariable("MattEnbiluluServer"));
            var stream = client.GetStream(streamName);
            if (stream == null)
            {
                client.CreateStream(streamName);
            }
        }

        public void Publish(dynamic message)
        {
            client.PutRecord(streamName, message);
        }
    }
}
