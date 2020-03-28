using System;
namespace StrangeVanilla.Maat.lib.MessageBus
{
    public interface IMessageBus<T>
    {
        public void Publish(dynamic message);
    }
}
