using System;
using libEnbilulu;

namespace SV.Maat.lib.MessageBus
{
    public class EnbiluluBus : IMessageBus
    {
        public Enbilulu client;
        public string _streamName;
        
        public EnbiluluBus(string streamName)
        {
            _streamName = streamName;
            client = new Enbilulu();//Environment.GetEnvironmentVariable("MattEnbiluluServer"));
            var stream = client.GetStream(_streamName);
            if (stream == null)
            {
                client.CreateStream(_streamName);
            }
        }

        public void Publish(dynamic message)
        {
            client.PutRecord(_streamName, message);
        }
    }
}
