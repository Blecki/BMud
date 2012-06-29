using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class Evaluator
    {
        public delegate String DFunction(List<String> Tokens, EvaluationContext Context);

        private static Dictionary<String, DFunction> _registeredFunctions = new Dictionary<String, DFunction>();

        public static void AddFunction(String Name, DFunction Func)
        {
            if (_registeredFunctions.ContainsKey(Name.ToUpper())) throw new InvalidProgramException();
            _registeredFunctions.Add(Name.ToUpper(), Func);
        }

        public static String CallFunction(List<String> Tokens, EvaluationContext Context)
        {
            if (Tokens.Count == 0) return "";
            String Function = Tokens[0].ToUpper();
            if (!_registeredFunctions.ContainsKey(Function)) return "{UNKNOWN FUNCTION}";
            return _registeredFunctions[Function](Tokens, Context);
        }

        public static String _FuncObject(MudObject Object, List<String> Tokens, EvaluationContext Context)
        {
            if (Tokens.Count < 2) return "{NOT ENOUGH ARGUMENTS}";
            String Attribute = Tokens[1].ToUpper();
            if (Tokens.Count > 3) return "{TOO MANY ARGUMENTS}";
            String Default = Tokens.Count == 3 ? Tokens[2] : "";
            return EvaluateStringEx(Context._actor, Object, Context._object,
                Object.GetAttribute(Attribute, Default),
               Context._database, Context._operationLimit);
        }

        public static String FuncActor(List<String> Tokens, EvaluationContext Context)
        {
            return _FuncObject(Context._actor, Tokens, Context);
        }

        public static String FuncMe(List<String> Tokens, EvaluationContext Context)
        {
            return _FuncObject(Context._me, Tokens, Context);
        }

        public static String FuncObject(List<String> Tokens, EvaluationContext Context)
        {
            return _FuncObject(Context._object, Tokens, Context);
        }
    }
}
