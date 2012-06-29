using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public interface ICommandTokenMatcher
    {
        List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            String _rawCommand);
    }
}
