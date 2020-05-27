using System;
namespace SV.Maat.lib.MessageBus
{
    public interface IMessageBus
    {
        public void Publish(dynamic message);
    }
}
