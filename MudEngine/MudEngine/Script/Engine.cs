using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    public class Engine
    {
        public static bool PassesLock(
            String LockName,
            String DefaultValue,
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            IDatabaseService _database,
            IMessageService _message)
        {
            String Script = Me.GetAttribute(LockName, DefaultValue);
            if (String.IsNullOrEmpty(Script)) return true;

            var Variables = new VariableSet();
            Variables.Upsert("ACTOR", Actor);
            Variables.Upsert("ME", Me);
            Variables.Upsert("OBJECT", Object);
            Variables.Upsert("COUNT", new Integer(Object.Count));

            var ScriptResult = ExecuteScript(Me, Script, Variables, _database, _message);
            if (ScriptResult is String)
            {
                String Output = ScriptResult as String;
                Output = Evaluator.EvaluateString(Actor, Me, Object, Output, _database);
                _message.SendMessage(Actor, Output);
                return false;
            }
            else
                return true;
        }

        public static Object ExecuteScript(
            MudObject Executor,
            String Script,
            VariableSet Variables,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Context = new ExecutionContext();
            Context.Executor = Executor;
            Context._databaseService = _database;
            Context._messageService = _message;
            Context.OperationLimit = new OperationLimit { _limit = 256 };
            Context.Variables = Variables;

            var Node = Parser.ParseScript(Script, 0);
            if (Node != null) return Node.Execute(Context);
            return null;
        }
    }
}
