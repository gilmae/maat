using System;
namespace SV.Maat.Commands
{
    public abstract class BaseCommand<T>
    {
        public abstract T Execute();
    }
}
