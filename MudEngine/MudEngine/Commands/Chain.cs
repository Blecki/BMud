using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Chain : ICommandProcessor
    {
        ICommandProcessor First;
        ICommandProcessor Second;

        public Chain(ICommandProcessor F, ICommandProcessor S) { First = F; Second = S; }

        public void Perform(PossibleMatch _match, IDatabaseService _database, IMessageService _message)
        {
            First.Perform(_match, _database, _message);
            Second.Perform(_match, _database, _message);
        }
    }
}
