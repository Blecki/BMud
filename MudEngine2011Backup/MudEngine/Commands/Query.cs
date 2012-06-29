using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Query : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            String Key = _match.GetArgument<String>("KEY", null);
            String Value = _match.GetArgument<String>("VALUE", null);

            var Results = _database.FindObjects(Key, Value ?? "");

            if (Results.Count == 0) _message.SendMessage(Actor, "Found no objects.\n");
            else
            {
                String Output = "Found " + Results.Count.ToString() + " objects. ";
                foreach (var Item in Results) Output += Item.ToString() + ":";
                _message.SendMessage(Actor, Output + "\n");
            }
        }
    }

    public class QueryTimers : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Of = _match.GetArgument<MudObject>("OF", null);

            var Results = _database.QueryObjectTimers(Of.ID);
            if (Results.Count == 0) _message.SendMessage(Actor, "Found no timers.\n");
            else
            {
                foreach (var Item in Results)
                    _message.SendMessage(Actor, "#" + Item.ObjectID.ToString() + " in " +
                        (Item.Tick - _message.GetNow()).ToString() + " ticks : " + Item.Attribute + "\n");
            }
        }
    }

    public class StopTimers : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Of = _match.GetArgument<MudObject>("OF", null);

            if (MudCore.CheckPermission(Actor, Of, _database))
            {
                _database.StopTimers(Of.ID);
                _message.SendMessage(Actor, "Stopped.\n");
            }
            else
                _message.SendMessage(Actor, "You do not have permission to modify that object.\n");

        }
    }

}
