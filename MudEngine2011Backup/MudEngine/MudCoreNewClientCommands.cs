using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        private int client_id = 0;

        public void NewClientConnected(Client _client)
        {
            _client._id = (client_id++);
            Console.WriteLine("Gained client " + _client._id.ToString() + "\n");
            _client.Status = ClientStatus.NewClient;
            _client.Send("      *    /# __    ##\\      #          \n");
            _client.Send("      ||   ##-##    ###  ##_  #         \n");
            _client.Send("    _/ /---##-##_##-###_|--#---\\__      \n");
            _client.Send("--------------- Port Ivy ---------------\n");
            _client.Send("Returning players, login name password. \n");
            _client.Send("New players, register name password.    \n");
        }

        public void ClientDisconnected(Client _client)
        {
            Console.WriteLine("Lost client " + _client._id.ToString() + "\n");
            if (_client.Status == ClientStatus.LoggedOn)
            {
                _databaseLock.WaitOne();
                var Player = MudObject.FromID(_client.PlayerObject, DatabaseService);
                MudObject.MoveObject(Player, null, "", 1);
                ConnectedClients.Remove(_client.PlayerObject);
                DatabaseService.CommitChanges();
                _databaseLock.ReleaseMutex();
            }
        }

        private void Register(Client _client, String Name, String Password)
        {
            Int64 PlayerObject = DatabaseConstants.Invalid;
            String _Password = "";

            _databaseLock.WaitOne();
            bool AccountExists = AccountService.QueryAccount(Name, out _Password, out PlayerObject);
            if (AccountExists)
            {
                _databaseLock.ReleaseMutex();
                _client.Send("That account already exists.\n");
                return;
            }

            PlayerObject = MudCore.CreateNewAccountPlayerObject(Name, DatabaseService);
            AccountService.ModifyAccount(Name, Password, PlayerObject);

            Login(_client, Name, Password);

            _databaseLock.ReleaseMutex();
        }

        private void Login(Client _client, String Name, String Password)
        {
            Int64 PlayerObject = DatabaseConstants.Invalid;
            String StoredPassword = "";

            _databaseLock.WaitOne();

            bool AccountExists = AccountService.QueryAccount(Name, out StoredPassword, out PlayerObject);
            if (!AccountExists || Password != StoredPassword)
            {
                _client.Send("Username or password is wrong.\n");
                _databaseLock.ReleaseMutex();
                return;
            }

            if (ConnectedClients.ContainsKey(PlayerObject))
            {
                _client.Send("You are already connected.\n");
                _databaseLock.ReleaseMutex();
                return;
            }

            try
            {
                var Player = MudObject.FromID(PlayerObject, DatabaseService);
                MudObject.MoveObject(Player, MudObject.FromID(1, DatabaseService), "IN", 1);
                var TimerList = DatabaseService.QueryObjectTimers(PlayerObject);
                if (TimerList.Count((A) => { return A.Attribute == "@PLAYER"; }) == 0)
                    DatabaseService.StartTimer(GetFutureTime(6), PlayerObject, "@PLAYER");

                _client.Status = ClientStatus.LoggedOn;
                _client.PlayerObject = PlayerObject;
                ConnectedClients.Add(PlayerObject, _client);
                DatabaseService.CommitChanges();
            }
            catch (Exception)
            {
                DatabaseService.DiscardChanges();
                throw;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            _commandLock.WaitOne();
            PendingCommands.AddLast(new MudEngine.Command
            {
                Executor = PlayerObject,
                _Command = "LOOK"
            });
            _commandLock.ReleaseMutex();

        }
            


        private void NewClientCommand(Client _client, String _command)
        {
            try
            {
                var Tokens = new List<String>(_command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                if (Tokens.Count < 1)
                {
                    _client.Send("Valid commands are login or register.\n");
                    return;
                }

                if (Tokens[0].ToUpper() == "REGISTER")
                {
                    if (Tokens.Count != 3)
                    {
                        _client.Send("That is not the correct number of parameters. Register Name Password\n");
                        return;
                    }

                    Register(_client, Tokens[1], Tokens[2]);
                }
                else if (Tokens[0].ToUpper() == "LOGIN")
                {
                    if (Tokens.Count != 3)
                    {
                        _client.Send("That is not the correct number of parameters. Login Name Password\n");
                        return;
                    }

                    Login(_client, Tokens[1], Tokens[2]);
                }
                else
                    _client.Send("Valid commands are login or register.\n");
            }
            catch (Exception e)
            {
                _client.Send(e.Message + "\n" + e.StackTrace + "\n");
            }
        }
    }
}
