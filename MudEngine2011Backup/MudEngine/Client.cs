using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    internal enum ClientStatus
    {
        NewClient,
        LoggedOn,
        Disconnected
    }

    public class Client
    {
        public virtual void Send(String message) { }
        public virtual void Disconnect() { }

        internal Int32 _id;
        internal Int64 PlayerObject;
        internal ClientStatus Status = ClientStatus.NewClient; 
    }
}
