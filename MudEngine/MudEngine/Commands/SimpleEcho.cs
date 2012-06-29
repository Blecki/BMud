using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class SimpleEcho : ICommandProcessor
    {
        String text;

        public SimpleEcho(String text) { this.text = text; }

        public void Perform(PossibleMatch _match, IDatabaseService _database, IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            _message.SendMessage(Actor, text);
        }
    }
}
