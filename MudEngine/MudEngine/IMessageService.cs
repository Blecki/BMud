using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public interface IMessageService
    {
        void SendMessage(MudObject Object, String _message);
        void Command(Int64 Executor, String Command);
        bool IsPlayerConnected(Int64 ID);
        Int64 GetFutureTime(Int64 Ticks);
        Int64 GetNow();
    }
}
