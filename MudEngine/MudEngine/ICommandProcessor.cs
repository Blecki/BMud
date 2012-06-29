using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public interface ICommandProcessor
    {
        void Perform(PossibleMatch _match,
            IDatabaseService _database, 
            IMessageService _message);
    }
}
