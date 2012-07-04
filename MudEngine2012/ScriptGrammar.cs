using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace MudEngine2012
{
    [Language("MudScript", "0.1", "Scripting language for 2012 Mud Engine")]
    internal class ScriptGrammar : Irony.Parsing.Grammar
    {
        public ScriptGrammar()
        {
            //this.WhitespaceChars = String.Empty;

            var comment = new CommentTerminal("comment", "/*", "*/");
            NonGrammarTerminals.Add(comment);

            var token = TerminalFactory.CreateCSharpIdentifier("Token");
            
            var stringLiteral = TerminalFactory.CreateCSharpString("String");
            var integerLiteral = TerminalFactory.CreateCSharpNumber("Integer");
            integerLiteral.Options = NumberOptions.AllowSign | NumberOptions.IntOnly;

            var memberAccess = new NonTerminal("Member Access");
            var expression = new NonTerminal("Expression");
            var argumentList = new NonTerminal("Argument List");
            var node = new NonTerminal("Node");
            var stringPart = new NonTerminal("String Part");
            var whitespace = new NonTerminal("Whitespace");
            var embeddedString = new NonTerminal("Embedded String");
            
            var textLiteral = new FreeTextLiteral("Text Literal", FreeTextOptions.AllowEof, "(", "^", "*", "\"", "/", "$");
            textLiteral.Escapes.Add("\\^", "^");
            textLiteral.Escapes.Add("\\(", "(");
            textLiteral.Escapes.Add("\\\"", "\"");
            textLiteral.Escapes.Add("\\*", "*");
            textLiteral.Escapes.Add("\\/", "/");
            textLiteral.Escapes.Add("\\$", "$");
            textLiteral.Escapes.Add("\\n", "\n");
            textLiteral.Escapes.Add("\\\\", "\\");
            var root = new NonTerminal("Root");

            //whitespace.Rule = MakeStarRule(whitespace, ToTerm(" ") | "\n" | "\t" | "\r");
            //whitespace.Precedence = 10;

            memberAccess.Rule = expression + (ToTerm(":") | ".") + (token | node);
            argumentList.Rule = MakeStarRule(argumentList, expression);
            node.Rule = (ToTerm("^") | "*" | "$").Q() + "(" + argumentList + ")";
            embeddedString.Rule = ToTerm("^").Q() + "\"" + root + "\"";
            expression.Rule = token | integerLiteral | embeddedString | memberAccess | node;
            stringPart.Rule = node | textLiteral;
            root.Rule = MakeStarRule(root, stringPart);

            this.Root = root;

            this.RegisterBracePair("(", ")");
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            //this.MarkPunctuation(":");
            this.MarkTransient(expression, stringPart);
            this.RegisterOperators(8, Associativity.Right, ":", ".");

        }

    }
}