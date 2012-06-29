using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class Evaluator
    {
        public class EvaluationContext
        {
            public MudObject _actor;
            public MudObject _me;
            public MudObject _object;
            public IDatabaseService _database;
            internal OperationLimit _operationLimit;

            public String _EvalEx(String str)
            {
                return Evaluator.EvaluateStringEx(_actor, _me, _object, str, _database, _operationLimit);
            }
        }

        public static String EvaluateAttribute(
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            String Attribute,
            String Default,
            IDatabaseService Database)
        {
            return EvaluateString(Actor, Me, Object, Me.GetAttribute(Attribute, Default), Database);
        }

        internal static String EvaluateAttributeEx(
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            String Attribute,
            String Default,
            IDatabaseService Database,
            OperationLimit _operationLimit)
        {
            return EvaluateStringEx(Actor, Me, Object, Me.GetAttribute(Attribute, Default), Database, _operationLimit);
        }

        public static String EvaluateString(
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            String String,
            IDatabaseService Database)
        {
            return _EvaluateString(String, new EvaluationContext
            {
                _actor = Actor,
                _me = Me,
                _object = Object,
                _database = Database,
                _operationLimit = new OperationLimit { _limit = 256 }
            });
        }

        internal static String EvaluateStringEx(
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            String String,
            IDatabaseService Database,
            OperationLimit _operationLimit)
        {
            return _EvaluateString(String, new EvaluationContext
            {
                _actor = Actor,
                _me = Me,
                _object = Object,
                _database = Database,
                _operationLimit = _operationLimit
            });
        }

        private static List<String> ParseArguments(String _str)
        {
            List<String> Result = new List<String>();

            String Temp = "";
            int Place = 0;
            int Depth = 0;

            while (Place < _str.Length)
            {
                if (_str[Place] == ':' && Depth == 0)
                {
                    if (!String.IsNullOrEmpty(Temp)) { Result.Add(Temp); Temp = ""; }
                }
                else
                {
                    if (_str[Place] == '<') ++Depth;
                    if (_str[Place] == '>') --Depth;

                    Temp += _str[Place];
                }
                ++Place;
            }

            if (!String.IsNullOrEmpty(Temp)) Result.Add(Temp);

            return Result;
        }

        private static String EvaluateFunction(String _string, EvaluationContext Context)
        {
            var Tokens = ParseArguments(_string);

            if (Tokens.Count > 0 && Tokens[0][0] == '#')
            {
                try
                {
                    var ID = Convert.ToInt64(Tokens[0].Substring(1));
                    var Object = MudObject.FromID(ID, Context._database);
                    return _FuncObject(Object, Tokens, Context);
                }
                catch (Exception) { return "{ERROR PARSING ID}"; }
            }

            return CallFunction(Tokens, Context);
        }

        private class EvalResult
        {
            public String Result = "";
            public bool CapFlag = false;

            public void Append(char c)
            {
                if (CapFlag)
                {
                    Result += new String(c, 1).ToUpper();
                    CapFlag = false;
                }
                else Result += new String(c, 1);
            }

            public void Append(String s)
            {
                foreach (var c in s) Append(c);
            }
        }

        private static String _EvaluateString(String _string, EvaluationContext Context)
        {
            if (Context._operationLimit.Dec()) throw new OperationLimitExceededException();
            EvalResult Result = new EvalResult();
            int Place = 0;

            while (Place < _string.Length)
            {
                if (_string[Place] == '<')
                {
                    //Find matching close brace
                    String Temp = "";
                    int Depth = 0;
                    ++Place;

                    while (Place < _string.Length)
                    {
                        if (_string[Place] == '<') ++Depth;
                        if (_string[Place] == '>')
                        {
                            if (Depth == 0) break;
                            else --Depth;
                        }

                        Temp += _string[Place];
                        ++Place;
                    }

                    ++Place; //Skip closing >
                    if (!String.IsNullOrEmpty(Temp)) Result.Append(EvaluateFunction(Temp, Context));
                }
                else if (_string[Place] == '\\')
                {
                    ++Place;
                    if (Place >= _string.Length) return Result.Result;
                    if (_string[Place] == 'n') Result.Append("\n");
                    else if (_string[Place] == ':') Result.Append(':');
                    else if (_string[Place] == '\\') Result.Append('\\');
                    ++Place;
                }
                else if (_string[Place] == '^')
                {
                    ++Place;
                    Result.CapFlag = true;
                }
                else
                {
                    Result.Append(_string[Place]);
                    ++Place;
                }
            }

            return Result.Result;
        }
    }
}
