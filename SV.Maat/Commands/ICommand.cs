using System;
using Events;

namespace SV.Maat.Commands
{
    public interface ICommand
    {
        bool IsValid(Aggregate aggregate);
        Event GetEvent(int version);
    }

    public static class ICommandExtensions
    {
        public static (Event, bool) AttemptCommand(this Aggregate aggregate, ICommand command)
        {
            if (command.IsValid(aggregate))
            {
                return (command.GetEvent(aggregate.Version++), true);
            }
            return (null, false);
        }
    }
}
