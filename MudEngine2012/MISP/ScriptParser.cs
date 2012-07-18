using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public class ParseNode
    {
        public String type = "";
        public String token = "";
        public List<ParseNode> childNodes = new List<ParseNode>();
        public int start;
        public int end;

        public ParseNode(String type, int start) { this.type = type; this.start = start; }

        public void DebugEmit(int depth)
        {
            Console.WriteLine(new String(' ', depth) + type + ": [" + token + "]");
            foreach (var child in childNodes) child.DebugEmit(depth + 1);
        }
    }

    public class ParseError : ScriptError
    {
        public ParseError(String msg) : base(msg) {}
    }

    public class ParseState
    {
        public int start;
        public int end;
        public String source;

        public char Next() { return source[start]; }
        public void Advance(int distance = 1) { start += distance; if (start > end) throw new ParseError("Unexpected end of script"); }
        public bool AtEnd() { return start == end; }

        public bool MatchNext(String str) 
        {
            if (str.Length + start > source.Length) return false;
            return str == source.Substring(start, str.Length); 
        }
    }

    public class ScriptParser
    {
        public static bool IsWhitespace(char c)
        {
            return " \t\r\n".Contains(c);
        }

        public static void DevourWhitespace(ParseState state)
        {
            while (!state.AtEnd() && " \t\r\n".Contains(state.Next())) state.Advance();
        }

        public static ParseNode ParseToken(ParseState state)
        {
            var result = new ParseNode("token", state.start);
            while (!state.AtEnd() && !(" \t\r\n:.)".Contains(state.Next()))) state.Advance();
            result.end = state.start;
            result.token = state.source.Substring(result.start, result.end - result.start);
            return result;
        }

        public static ParseNode ParseInteger(ParseState state)
        {
            var result = new ParseNode("integer", state.start);
            while (!state.AtEnd() && ("0123456789".Contains(state.Next()))) state.Advance();
            result.end = state.start;
            result.token = state.source.Substring(result.start, result.end - result.start);
            return result;
        }

        private class AccessChainNode
        {
            public ParseNode node;
            public String token;
        }

        public static ParseNode ReorderMemberAccessNode(ParseNode node)
        {
            //Convert (A (B (C D))) to (((A B) C) D)

            //Create an (A B C D) list.
            var nodeList = new LinkedList<AccessChainNode>();
            for (var n = node; n != null; n = (n.type == "member access" ? n.childNodes[1] : null))
            {
                if (n.type == "member access")
                    nodeList.AddLast(new AccessChainNode { node = n.childNodes[0], token = n.token });
                else
                    nodeList.AddLast(new AccessChainNode { node = n, token = "" });
            }

            //Each iteration, take the first two nodes and combine them into a new member access node.
            //(A B C D) becomes ((A B) C D), etc.
            while (nodeList.Count > 1)
            {
                var lhs = nodeList.First();
                nodeList.RemoveFirst();
                var rhs = nodeList.First();
                nodeList.RemoveFirst();

                var newNode = new ParseNode("member access", lhs.node.start);
                newNode.token = lhs.token;
                newNode.childNodes.Add(lhs.node);
                newNode.childNodes.Add(rhs.node);

                nodeList.AddFirst(new AccessChainNode
                {
                    node = newNode,
                    token = rhs.token
                });
            }

            return nodeList.First().node;
        }

        public static ParseNode ParseExpression(ParseState state)
        {
            ParseNode result = null;
            if (state.MatchNext("^\"")) //escaped string
            {
                state.Advance();
                var stringNode = ParseStringExpression(state);
                result = new ParseNode("string", stringNode.start)
                {
                    token = state.source.Substring(stringNode.start + 1, stringNode.end - stringNode.start - 2)
                };
            } 
            else if (state.Next() == '"')
            {
                result = ParseStringExpression(state);
            }
            else if (state.MatchNext("*(") 
                | state.MatchNext("^(")
                | state.MatchNext("$(") 
                | state.MatchNext("#(") 
                | state.MatchNext("("))
            {
                result = ParseNode(state);
            }
            else if ("0123456789".Contains(state.Next()))
            {
                result = ParseInteger(state);
            }
            else 
            {
                result = ParseToken(state);
            }
             
            if (state.Next() == '.' || state.Next() == ':')
            {
                var final_result = new ParseNode("member access", result.start);
                final_result.childNodes.Add(result);
                final_result.token = new String(state.Next(), 1);
                state.Advance();
                final_result.childNodes.Add(ParseExpression(state));
                return final_result;
            }
            return result;
        }
                        


        public static ParseNode ParseNode(ParseState state)
        {
            var result = new ParseNode("node", state.start);
            if (state.Next() == '*') { result.token = "*"; state.Advance(); }
            else if (state.Next() == '$') { result.token = "$"; state.Advance(); }
            else if (state.Next() == '^') { result.token = "^"; state.Advance(); }
            else if (state.Next() == '#') { result.token = "#"; state.Advance(); }
            if (state.Next() != '(') throw new ParseError("Expected (");
            state.Advance();
            while (state.Next() != ')')
            {
                DevourWhitespace(state);
                if (state.Next() != ')')
                {
                    var expression = ParseExpression(state);
                    if (expression.type == "member access") expression = ReorderMemberAccessNode(expression);
                    result.childNodes.Add(expression);
                }
                DevourWhitespace(state);
            }
            state.Advance();
            return result;
        }

        public static ParseNode ParseStringExpression(ParseState state, bool isRoot = false)
        {
            var result = new ParseNode("string expression", state.start);
            if (!isRoot) state.Advance(); //Skip opening quote
            string piece = "";
            int piece_start = state.start;
            while (!state.AtEnd())
            {
                if (state.Next() == '(') 
                {
                    if (piece.Length > 0) result.childNodes.Add(new ParseNode("string", piece_start) {
                        token = state.source.Substring(piece_start, state.start - piece_start) });
                    result.childNodes.Add(ParseNode(state));
                    piece = "";
                }
                else if (state.Next() == '\\')
                {
                    if (piece.Length == 0) piece_start = state.start;
                    state.Advance(); //skip the slash.
                    piece += "\\" + state.Next();
                    state.Advance();
                }
                else if (!isRoot && state.Next() == '"') 
                {
                    if (piece.Length > 0) result.childNodes.Add(new ParseNode("string", piece_start) {
                        token = state.source.Substring(piece_start, state.start - piece_start) });
                    state.Advance();
                    result.end = state.start;
                    if (result.childNodes.Count == 1 && result.childNodes[0].type == "string")
                        return result.childNodes[0];
                    return result;
                }
                else
                {
                    if (piece.Length == 0) piece_start = state.start;
                    piece += state.Next();
                    state.Advance();
                }

            }

            if (isRoot)
            {
                if (piece.Length > 0) result.childNodes.Add(new ParseNode("string", piece_start)
                {
                    token = state.source.Substring(piece_start, state.start - piece_start)
                });

                if (result.childNodes.Count == 1) return result.childNodes[0];
                return result;
            }
            
            throw new ParseError("Unexpected end of script inside string expression.");
        }

        public static void ParseComment(ParseState state)
        {
            state.Advance(2);
            while (!state.AtEnd() && !state.MatchNext("*/")) state.Advance();
            state.Advance(2);
        }

        public static ParseNode ParseRoot(String script)
        {
            var commentFree = "";
            var state = new ParseState { start = 0, end = script.Length, source = script };
            while (!state.AtEnd())
            {
                if (state.MatchNext("/*"))
                    ParseComment(state);
                else
                {
                    commentFree += state.Next();
                    state.Advance();
                }
            }

            return ParseStringExpression(new ParseState { start = 0, end = commentFree.Length, source = commentFree }, true);
        }
                
    }
}
