using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    public class Parser
    {
        internal class _True { }
        internal static _True True = new _True();

        public static SyntaxNode ParseScript(String _str, int depth)
        {
            var Arguments = new List<SyntaxNode>();

            int Place = 0;

            while (Place < _str.Length)
            {
                if (_str[Place] == '[')
                {
                    String Literal = "";
                    ++Place;
                    while (Place < _str.Length && _str[Place] != ']')
                    {
                        if (_str[Place] == '\\')
                        {
                            ++Place;
                            if (Place >= _str.Length) throw new RuntimeErrorException("Unclosed String Literal at " + Literal);
                            if (_str[Place] == '\\') Literal += '\\';
                            if (_str[Place] == 'n') Literal += '\n';
                            else Literal += _str[Place];
                        }
                        else
                            Literal += _str[Place];
                        ++Place;
                        if (Place >= _str.Length) throw new RuntimeErrorException("Unclosed String Literal at " + Literal);
                    }
                    Arguments.Add(new StringLiteral(Literal));
                    ++Place;
                }
                else if (_str[Place] == '(')
                {
                    ++Place;
                    String FunctionCall = "";
                    int Depth = 0;
                    while (Place < _str.Length)
                    {
                        if (_str[Place] == ')')
                        {
                            if (Depth == 0)
                            {
                                Arguments.Add(ParseScript(FunctionCall, depth + 1));
                                ++Place;
                                break;
                            }
                            else
                                --Depth;
                        }
                        if (_str[Place] == '(') ++Depth;
                        FunctionCall += _str[Place];
                        ++Place;
                        if (Place >= _str.Length)
                            throw new SyntaxErrorException("Unclosed paranthesis at : " + FunctionCall);
                    }
                }
                else if (_str[Place] != ' ')
                {
                    String Word = "";
                    while (Place < _str.Length && _str[Place] != ' ')
                    {
                        Word += _str[Place];
                        ++Place;
                    }

                    if (Word[0] >= '0' && Word[0] <= '9')
                    {
                        Int32 IntLiteral = 0;
                        for (int i = 0; i < Word.Length; ++i)
                        {
                            if (Word[i] < '0' || Word[i] > '9') throw new SyntaxErrorException("Bad integer literal : " + Word);
                            IntLiteral *= 10;
                            IntLiteral += (int)(Word[i] - '0');
                        }
                        Arguments.Add(new IntegerLiteral(IntLiteral));
                    }
                    else
                        Arguments.Add(new Identifier(Word));
                }
                else ++Place;
            }

            if (Arguments.Count < 1)
            {
                if (depth > 0) throw new SyntaxErrorException("Empty function call");
                else return null;
            }

            var Identifier = Arguments[0] as Identifier;
            if (Identifier == null)
            {
                if (depth > 0) throw new SyntaxErrorException("Function call must start with function name.");
                else if (Arguments.Count > 1) throw new SyntaxErrorException("Too many arguments.");
                else return Arguments[0];
            }

            try
            {
                Object FuncObject = Activator.CreateInstance(Type.GetType("MudEngine.Script." + Identifier._name.ToUpper()));
                var Function = FuncObject as FunctionCall;
                if (Function == null) throw new SyntaxErrorException("Unknown function : " + Identifier._name.ToUpper());
                Arguments.RemoveAt(0);
                Function.Children = new List<SyntaxNode>(Arguments);
                return Function;
            }
            catch (Exception)
            {
                throw new SyntaxErrorException("Unknown function : " + Identifier._name);
            }
        }
    }
}
