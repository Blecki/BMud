using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
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

        internal String path;
        internal MudObject PlayerObject;
        internal ClientStatus Status = ClientStatus.NewClient; 
    }
}
