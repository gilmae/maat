using System;
using libEnbilulu;
using Microsoft.Extensions.Configuration;

namespace SV.Maat.lib.MessageBus
{
    public class EnbiluluBus : IMessageBus
    {
        public Client client;
        public string _streamName;
        
        public EnbiluluBus(string streamName, IConfiguration configuration)
        {
            _streamName = streamName;
            Uri enbiluluUri = new Uri(configuration.GetConnectionString("Enbilulu"));
            client = new Client(enbiluluUri.Host, enbiluluUri.Port);
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
