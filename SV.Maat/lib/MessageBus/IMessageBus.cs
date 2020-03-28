using System;
namespace SV.Maat.lib.MessageBus
{
    public interface IMessageBus<T>
    {
        public void Publish(dynamic message);
    }
}
