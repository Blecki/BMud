using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public partial class MudCore
    {
        public void ClientDisconnected(Client _client)
        {

            if (_client.Status == ClientStatus.LoggedOn)
            {
                Console.WriteLine("Lost client " + _client.path.ToString() + "\n");
                _databaseLock.WaitOne();
                //var Player = MudObject.FromID(_client.PlayerObject, DatabaseService);
                //MudObject.MoveObject(Player, null, "", 1);
                ConnectedClients.Remove(_client.path);
                //DatabaseService.CommitChanges();
                _databaseLock.ReleaseMutex();
            }
            else
                Console.WriteLine("Lost un-logged-in client.\n");
        }

        private void Login(Client _client, String Name, String Password)
        {
            _databaseLock.WaitOne();
            var playerObject = database.LoadObject(Name, null);
            if (playerObject == null || Password != playerObject.GetAttribute("password").ToString())
            {
                _client.Send("Username or password is wrong.\n");
                _databaseLock.ReleaseMutex();
                return;
            }

            if (ConnectedClients.ContainsKey(Name))
            {
                _client.Send("You are already connected.\n");
                _databaseLock.ReleaseMutex();
                return;
            }

            try
            {
                //var Player = MudObject.FromID(PlayerObject, DatabaseService);
                //MudObject.MoveObject(Player, MudObject.FromID(1, DatabaseService), "IN", 1);
                //var TimerList = DatabaseService.QueryObjectTimers(PlayerObject);
                _client.path = Name;
                _client.Status = ClientStatus.LoggedOn;
                _client.PlayerObject = playerObject;
                ConnectedClients.Add(Name, _client);

                playerObject.SetAttribute("location", database.LoadObject("room", playerObject));
                //DatabaseService.CommitChanges();
            }
            catch (Exception)
            {
                //DatabaseService.DiscardChanges();
                throw;
            }
            finally
            {
                _databaseLock.ReleaseMutex();
            }

            //_commandLock.WaitOne();
            //PendingCommands.AddLast(new MudEngine.Command
            //{
            //    Executor = PlayerObject,
            //    _Command = "LOOK"
            //});
            //_commandLock.ReleaseMutex();

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

                if (Tokens[0].ToUpper() == "LOGIN")
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
                _client.Send("[NewClientCommand] " + e.Message + "\n" + e.StackTrace + "\n");
            }
        }
    }
}
