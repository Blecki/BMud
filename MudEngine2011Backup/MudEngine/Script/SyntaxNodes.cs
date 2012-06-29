using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    public abstract class SyntaxNode
    {
        internal abstract Object Execute(ExecutionContext Context);
        internal abstract void Emit(MudObject Actor, IMessageService MessageService, int Depth);
    }

    internal abstract class FunctionCall : SyntaxNode
    {
        internal List<SyntaxNode> Children = new List<SyntaxNode>();

        internal override void Emit(MudObject Actor, IMessageService MessageService, int Depth)
        {
            String Output = new String(' ', Depth * 4);
            Output += this.GetType().Name + "\n";
            MessageService.SendMessage(Actor, Output);
            foreach (var Child in Children) Child.Emit(Actor, MessageService, Depth + 1);
        }
    }

    internal class StringLiteral : SyntaxNode
    {
        internal String _value;
        internal StringLiteral(String Value) { _value = Value; }
        internal override object Execute(ExecutionContext Context)
        {
            return _value;
        }

        internal override void Emit(MudObject Actor, IMessageService MessageService, int Depth)
        {
            MessageService.SendMessage(Actor, new String(' ', Depth * 4) + "[" + _value + "]\n");
        }
    }

    internal class Integer
    {
        public Int32 Value;
        public Integer(Int32 Value) { this.Value = Value; }
    }

    internal class IntegerLiteral : SyntaxNode
    {
        public Int32 Value;
        public IntegerLiteral(Int32 Value) { this.Value = Value; }
        internal override object Execute(ExecutionContext Context)
        {
            return new Integer(Value);
        }

        internal override void Emit(MudObject Actor, IMessageService MessageService, int Depth)
        {
            MessageService.SendMessage(Actor, new String(' ', Depth * 4) + "int " + Value.ToString() + "\n");
        }
    }

    internal class Identifier : SyntaxNode
    {
        internal String _name;
        internal Identifier(String Name) { _name = Name; }

        internal override object Execute(ExecutionContext Context)
        {
            if (_name.ToUpper() == "NULL") return null;
            else if (_name[0] == '#')
            {
                //Referring to a specific entity.
                try
                {
                    return MudObject.FromID(Convert.ToInt64(_name.Substring(1)), Context._databaseService);
                }
                catch (Exception) { return null; }
            }
            else return Context.Variables.GetRawArgument(_name.ToUpper());
        }

        internal override void Emit(MudObject Actor, IMessageService MessageService, int Depth)
        {
            MessageService.SendMessage(Actor, new String(' ', Depth * 4) + "var " + _name + "\n");
        }
    }

    internal class NOP : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            foreach (var Node in Children) Node.Execute(Context);
            return null;
        }
    }
}
