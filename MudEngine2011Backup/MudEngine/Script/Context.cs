using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    public class VariableSet
    {
        Dictionary<String, Object> Variables;

        public VariableSet()
        {
            Variables = new Dictionary<String, Object>();
        }

        public T GetArgument<T>(String Name, T DefaultValue)
        {
            if (!Variables.ContainsKey(Name)) return DefaultValue;
            else return (T)Variables[Name];
        }

        public Object GetRawArgument(String Name)
        {
            if (!Variables.ContainsKey(Name)) return null;
            else return Variables[Name];
        }

        public void Upsert(String Name, Object Value)
        {
            if (Variables.ContainsKey(Name)) Variables[Name] = Value;
            else Variables.Add(Name, Value);
        }

        public VariableSet Clone()
        {
            return new VariableSet { Variables = new Dictionary<String, Object>(this.Variables) };
        }
    }

    internal class ExecutionContext
    {
        public MudObject Executor;
        public IMessageService _messageService;
        public IDatabaseService _databaseService;
        public OperationLimit OperationLimit;
        public VariableSet Variables;
    }
}
