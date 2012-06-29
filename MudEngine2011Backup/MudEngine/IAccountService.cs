using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    
    public interface IAccountService
    {
        
        void ModifyAccount(String Name, String Password, Int64 PlayerObject);
        bool QueryAccount(String Name, out String Password, out Int64 PlayerObject);
    }
}
