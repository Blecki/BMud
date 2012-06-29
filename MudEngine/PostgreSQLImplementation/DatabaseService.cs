using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace PostgreSQLImplementation
{
    public class DatabaseService : MudEngine.IDatabaseService, MudEngine.IAccountService
    {
        internal NpgsqlConnection _databaseConnection;
        internal NpgsqlTransaction _transaction;
        internal NpgsqlCommand _upsertCommand;
        internal NpgsqlCommand _deleteCommand;
        internal NpgsqlCommand _querySingleCommand;
        internal NpgsqlCommand _queryAllCommand;
        internal NpgsqlCommand _deleteObjectCommand;
        internal NpgsqlCommand _findObjectsCommand;

        internal NpgsqlCommand _upsertAccountCommand;
        internal NpgsqlCommand _queryAccountCommand;

        internal NpgsqlCommand _startTimer;
        internal NpgsqlCommand _queryObjectTimers;
        internal NpgsqlCommand _queryDueTimers;
        internal NpgsqlCommand _stopTimers;
        internal NpgsqlCommand _clearOldTimers;

        public DatabaseService()
        {
            _databaseConnection = new NpgsqlConnection("Server=Localhost;Port=5432;User Id=Tony;Database=MudDatabase;");
            _databaseConnection.Open();

            _upsertCommand = new NpgsqlCommand("SELECT upsert(:ID, :KEY, :VALUE);", _databaseConnection);
            _upsertCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));
            _upsertCommand.Parameters.Add(new NpgsqlParameter("KEY", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertCommand.Parameters.Add(new NpgsqlParameter("VALUE", NpgsqlTypes.NpgsqlDbType.Text));

            _deleteCommand = new NpgsqlCommand(
                "DELETE FROM \"Objects\" WHERE \"Object\"=:ID AND \"Key\"=:Key;", _databaseConnection);
            _deleteCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));
            _deleteCommand.Parameters.Add(new NpgsqlParameter("KEY", NpgsqlTypes.NpgsqlDbType.Text));

            _querySingleCommand = new NpgsqlCommand(
                "SELECT * FROM \"Objects\" WHERE \"Object\"=:ID AND \"Key\"=:KEY;", _databaseConnection);
            _querySingleCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));
            _querySingleCommand.Parameters.Add(new NpgsqlParameter("KEY", NpgsqlTypes.NpgsqlDbType.Text));

            _queryAllCommand = new NpgsqlCommand("SELECT * FROM \"Objects\" WHERE \"Object\"=:ID;", _databaseConnection);
            _queryAllCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));

            _deleteObjectCommand = new NpgsqlCommand("DELETE FROM \"Objects\" WHERE \"Object\"=:ID;", _databaseConnection);
            _deleteObjectCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));

            _upsertAccountCommand = new NpgsqlCommand("SELECT upsert_account(:NAME, :PASSWORD, :PLAYEROBJECT);", _databaseConnection);
            _upsertAccountCommand.Parameters.Add(new NpgsqlParameter("NAME", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertAccountCommand.Parameters.Add(new NpgsqlParameter("PASSWORD", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertAccountCommand.Parameters.Add(new NpgsqlParameter("PLAYEROBJECT", NpgsqlTypes.NpgsqlDbType.Bigint));

            _queryAccountCommand = new NpgsqlCommand("SELECT * FROM \"Accounts\" WHERE \"Name\"=:NAME;", _databaseConnection);
            _queryAccountCommand.Parameters.Add(new NpgsqlParameter("NAME", NpgsqlTypes.NpgsqlDbType.Text));

            _findObjectsCommand = new NpgsqlCommand("SELECT \"Object\" FROM \"Objects\" WHERE \"Key\"=:KEY AND \"Value\"=:VALUE;", _databaseConnection);
            _findObjectsCommand.Parameters.Add(new NpgsqlParameter("KEY", NpgsqlTypes.NpgsqlDbType.Text));
            _findObjectsCommand.Parameters.Add(new NpgsqlParameter("VALUE", NpgsqlTypes.NpgsqlDbType.Text));

            _startTimer = new NpgsqlCommand("INSERT INTO \"Timers\" (\"Object\", \"Tick\", \"Attribute\") VALUES (:OBJECT, :TICK, :ATTRIBUTE);", _databaseConnection);
            _startTimer.Parameters.Add(new NpgsqlParameter("OBJECT", NpgsqlTypes.NpgsqlDbType.Bigint));
            _startTimer.Parameters.Add(new NpgsqlParameter("TICK", NpgsqlTypes.NpgsqlDbType.Bigint));
            _startTimer.Parameters.Add(new NpgsqlParameter("ATTRIBUTE", NpgsqlTypes.NpgsqlDbType.Text));

            _queryObjectTimers = new NpgsqlCommand("SELECT * FROM \"Timers\" WHERE \"Object\"=:OBJECT;", _databaseConnection);
            _queryObjectTimers.Parameters.Add(new NpgsqlParameter("OBJECT", NpgsqlTypes.NpgsqlDbType.Bigint));

            _queryDueTimers = new NpgsqlCommand("SELECT * FROM \"Timers\" WHERE \"Tick\"<=:TICK;", _databaseConnection);
            _queryDueTimers.Parameters.Add(new NpgsqlParameter("TICK", NpgsqlTypes.NpgsqlDbType.Bigint));

            _stopTimers = new NpgsqlCommand("DELETE FROM \"Timers\" WHERE \"Object\"=:OBJECT;", _databaseConnection);
            _stopTimers.Parameters.Add(new NpgsqlParameter("OBJECT", NpgsqlTypes.NpgsqlDbType.Text));

            _clearOldTimers = new NpgsqlCommand("DELETE FROM \"Timers\" WHERE \"Tick\"<=:TICK;", _databaseConnection);
            _clearOldTimers.Parameters.Add(new NpgsqlParameter("TICK", NpgsqlTypes.NpgsqlDbType.Bigint));

            _transaction = _databaseConnection.BeginTransaction();

            Console.WriteLine("Connected to PostgreSQL Database.\n");
        }

        private Int64 _cachedNextObjectID = -1;

        private Int64 GetNextObjectID()
        {
            if (_cachedNextObjectID != -1) return _cachedNextObjectID;
            try
            {
                return Convert.ToInt64(QueryAttribute(MudEngine.DatabaseConstants.God, "DATABASE_NEXT_OBJECT_ID", "0"));
            }
            catch (Exception) { return 0; }
        }

        public bool ValidID(Int64 ID) { return CheckID(ID); }

        private bool CheckID(Int64 ID)
        {
            return ID >= MudEngine.DatabaseConstants.Money && ID < GetNextObjectID();
        }

        public String QueryAttribute(Int64 ID, String Key, String DefaultValue)
        {
            _querySingleCommand.Parameters["ID"].Value = ID;
            _querySingleCommand.Parameters["KEY"].Value = Key.ToUpper();

            using (var Reader = _querySingleCommand.ExecuteReader())
            {
                if (!Reader.HasRows) return DefaultValue;
                Reader.Read();
                return Reader[2].ToString();
            }
        }

        public Dictionary<String, String> GetAllAttributes(Int64 ID)
        {
            _queryAllCommand.Parameters["ID"].Value = ID;
            using (var Reader = _queryAllCommand.ExecuteReader())
            {
                var Result = new Dictionary<String, String>();
                while (Reader.Read())
                    Result.Add(Reader[1].ToString(), Reader[2].ToString());
                return Result;
            }
        }

        public bool HasAttribute(Int64 ID, String Key)
        {
            _querySingleCommand.Parameters["ID"].Value = ID;
            _querySingleCommand.Parameters["KEY"].Value = Key.ToUpper();

            using (var Reader = _querySingleCommand.ExecuteReader())
            {
                return Reader.HasRows;
            }
        }

        public void WriteAttribute(Int64 ID, String Key, String Value)
        {
            if (CheckID(ID))
            {
                _upsertCommand.Parameters["ID"].Value = ID;
                _upsertCommand.Parameters["KEY"].Value = Key.ToUpper();
                _upsertCommand.Parameters["VALUE"].Value = Value;
                _upsertCommand.ExecuteNonQuery();
            }
        }

        public void RemoveAttribute(Int64 ID, String Key)
        {
            if (CheckID(ID))
            {
                _deleteCommand.Parameters["ID"].Value = ID;
                _deleteCommand.Parameters["KEY"].Value = Key.ToUpper();
                _deleteCommand.ExecuteNonQuery();
            }
        }

        public Int64 CreateObject()
        {
            Int64 NewObjectID = GetNextObjectID();
            WriteAttribute(MudEngine.DatabaseConstants.God, "DATABASE_NEXT_OBJECT_ID", (NewObjectID + 1).ToString());
            _cachedNextObjectID = -1;
            return NewObjectID;
        }

        public bool DestroyObject(Int64 ID)
        {
            return false;
        }

        public void CommitChanges()
        {
            if (_transaction != null) _transaction.Commit();
            _transaction = _databaseConnection.BeginTransaction();
            _cachedNextObjectID = -1;
        }

        public void DiscardChanges()
        {
            if (_transaction != null) _transaction.Rollback();
            _transaction = _databaseConnection.BeginTransaction();
            _cachedNextObjectID = -1;
        }

        public void ModifyAccount(String Name, String Password, Int64 PlayerObject)
        {
            _upsertAccountCommand.Parameters["NAME"].Value = Name;
            _upsertAccountCommand.Parameters["PASSWORD"].Value = Password;
            _upsertAccountCommand.Parameters["PLAYEROBJECT"].Value = PlayerObject;
            _upsertAccountCommand.ExecuteNonQuery();
            CommitChanges();
        }

        public bool QueryAccount(String Name, out String Password, out Int64 PlayerObject)
        {
            Password = "";
            PlayerObject = -1;

            _queryAccountCommand.Parameters["NAME"].Value = Name;
            using (var Reader = _queryAccountCommand.ExecuteReader())
            {
                if (!Reader.HasRows) return false;
                Reader.Read();
                Password = Reader[2].ToString();
                PlayerObject = Reader.GetInt64(0);
                return true;
            }
        }

        public List<Int64> FindObjects(String Key, String Value)
        {
            _findObjectsCommand.Parameters["KEY"].Value = Key.ToUpper();
            _findObjectsCommand.Parameters["VALUE"].Value = Value;

            using (var Reader = _findObjectsCommand.ExecuteReader())
            {
                var Result = new List<Int64>();
                while (Reader.Read())
                    Result.Add(Reader.GetInt64(0));
                return Result;
            }
        }

        public void StartTimer(Int64 Tick, Int64 ID, String Value)
        {
            _startTimer.Parameters["OBJECT"].Value = ID;
            _startTimer.Parameters["TICK"].Value = Tick;
            _startTimer.Parameters["ATTRIBUTE"].Value = Value;
            _startTimer.ExecuteNonQuery();
        }

        public List<MudEngine.Timer> QueryObjectTimers(Int64 ID)
        {
            _queryObjectTimers.Parameters["OBJECT"].Value = ID;
            using (var Reader = _queryObjectTimers.ExecuteReader())
            {
                var Result = new List<MudEngine.Timer>();
                while (Reader.Read())
                    Result.Add(new MudEngine.Timer
                    {
                        ObjectID = ID,
                        Tick = Reader.GetInt64(1),
                        Attribute = Reader[2].ToString()
                    });
                return Result;
            }
        }

        public List<MudEngine.Timer> QueryDueTimers(Int64 Tick)
        {
            _queryDueTimers.Parameters["TICK"].Value = Tick;
            using (var Reader = _queryDueTimers.ExecuteReader())
            {
                var Result = new List<MudEngine.Timer>();
                while (Reader.Read())
                    Result.Add(new MudEngine.Timer
                    {
                        ObjectID = Reader.GetInt64(0),
                        Tick = Reader.GetInt64(1),
                        Attribute = Reader[2].ToString()
                    });
                return Result;
            }
        }

        public void StopTimers(Int64 ID)
        {
            _stopTimers.Parameters["OBJECT"].Value = ID;
            _stopTimers.ExecuteNonQuery();
        }

        public void ClearOldTimers(Int64 Tick)
        {
            _clearOldTimers.Parameters["TICK"].Value = Tick;
            _clearOldTimers.ExecuteNonQuery();
        }

    }
}
