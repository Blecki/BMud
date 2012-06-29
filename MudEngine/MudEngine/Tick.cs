using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static void TickPlayer(MudObject Player, IDatabaseService _database, IMessageService _message)
        {
            MudCore.GrantStat(Player, "STAMINA", 1);

            if (_message.IsPlayerConnected(Player.ID))
                _database.StartTimer(_message.GetFutureTime(6), Player.ID, "@PLAYER");

            _message.SendMessage(Player, "Tick!\n");
        }
    }
}