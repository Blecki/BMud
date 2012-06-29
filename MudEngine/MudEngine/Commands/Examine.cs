using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Examine : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Target = _match.GetArgument<MudObject>("OBJECT", null);

            if (Target.Instance)
                _message.SendMessage(_Actor, "Examining an instance of " + Target.ID.ToString() + "\n");
            else
                _message.SendMessage(_Actor, "Examining object " + Target.ID.ToString() + "\n");

            var Attributes = Target.GetAllAttributes();
            if (Attributes != null)
            {
                _message.SendMessage(_Actor, "Object Attributes : \n");
                foreach (var Pair in Attributes)
                {
                    String Output = "";
                    if (Target.IsContents(Pair.Key)) 
                    {
                        Output += " [C]" + Pair.Key + " = " + Pair.Value + "\n";
                        foreach (var Object in Target.GetContents(Pair.Key))
                            Output += " > " + Object.ToString() + "\n";
                    }
                    else Output += " " + Pair.Key + " = " + Pair.Value + "\n";
                    _message.SendMessage(_Actor, Output);
                }
            }

            Attributes = Target.GetAllLocalAttributes();
            if (Attributes != null)
            {
                _message.SendMessage(_Actor, "Local Attributes : \n");
                foreach (var Pair in Attributes)
                {
                    String Output = "";
                    if (Target.IsContents(Pair.Key))
                    {
                        Output += " [C]" + Pair.Key + " = " + Pair.Value + "\n";
                        foreach (var Object in Target.GetContents(Pair.Key))
                            Output += " > " + Object.ToString() + "\n";
                    }
                    else Output += " " + Pair.Key + " = " + Pair.Value + "\n";
                    _message.SendMessage(_Actor, Output);
                }
            }
        }
    }
}
