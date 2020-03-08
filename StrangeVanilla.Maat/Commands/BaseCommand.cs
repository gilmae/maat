using System;
namespace StrangeVanilla.Maat.Commands
{
    public abstract class BaseCommand<T>
    {
        public abstract T Execute();
    }
}
