using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
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

    public class Parser
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
            var result = new ParseNode(NodeType.Token, state.start);
            while (!state.AtEnd() && !(" \t\r\n:.)]".Contains(state.Next()))) state.Advance();
            result.end = state.start;
            result.token = state.source.Substring(result.start, result.end - result.start);
            if (String.IsNullOrEmpty(result.token)) throw new ParseError("Empty token");
            return result;
        }

        public static ParseNode ParseInteger(ParseState state)
        {
            var result = new ParseNode(NodeType.Integer, state.start);
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
            for (var n = node; n != null; n = (n.type == NodeType.MemberAccess ? n.childNodes[1] : null))
            {
                if (n.type == NodeType.MemberAccess)
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

                var newNode = new ParseNode(NodeType.MemberAccess, lhs.node.start);
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

        public static Prefix ParsePrefix(ParseState state)
        {
            Prefix result = Prefix.None;
            if (state.Next() == '*')
                result = Prefix.Quote;
            else if (state.Next() == '^')
                result = Prefix.List;
            else if (state.Next() == '$')
                result = Prefix.Expand;
            else if (state.Next() == '#')
                result = Prefix.Lookup;
            if (result != Prefix.None)
                state.Advance();
            return result;
        }

        public static ParseNode ParseExpression(ParseState state)
        {
            ParseNode result = null;
            var prefix = ParsePrefix(state);
            if (state.Next() == '[') //Dictionary Entry
            {
                result = ParseNode(state, "[", "]");
                result.type = NodeType.DictionaryEntry;
            }
            else if (state.Next() == '"')
            {
                result = ParseStringExpression(state);
                if (prefix == Prefix.Quote)
                    result = new ParseNode(NodeType.String, result.start)
                    {
                        token = state.source.Substring(result.start + 1, result.end - result.start - 2)
                    };
            }
            else if (state.Next() == '(')
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
                var final_result = new ParseNode(NodeType.MemberAccess, result.start);
                final_result.childNodes.Add(result);
                final_result.token = new String(state.Next(), 1);
                state.Advance();
                final_result.childNodes.Add(ParseExpression(state));
                result = final_result;
            }

            result.prefix = prefix;
            if (!PrefixCheck.CheckPrefix(result)) throw new ParseError("Illegal prefix on expression of type " + result.type);
            return result;
        }
                        


        public static ParseNode ParseNode(ParseState state, String start = "(", String end = ")")
        {
            var result = new ParseNode(NodeType.Node, state.start);
            if (!state.MatchNext(start)) throw new ParseError("Expected " + start);
            state.Advance(start.Length);
            while (!state.MatchNext(end))
            {
                DevourWhitespace(state);
                if (!state.MatchNext(end))
                {
                    var expression = ParseExpression(state);
                    if (expression.type == NodeType.MemberAccess) expression = ReorderMemberAccessNode(expression);
                    result.childNodes.Add(expression);
                }
                DevourWhitespace(state);
            }
            state.Advance(end.Length);
            return result;
        }

        public static ParseNode ParseStringExpression(ParseState state, bool isRoot = false)
        {
            var result = new ParseNode(NodeType.StringExpression, state.start);
            if (!isRoot) state.Advance(); //Skip opening quote
            string piece = "";
            int piece_start = state.start;
            while (!state.AtEnd())
            {
                if (state.Next() == '(') 
                {
                    if (piece.Length > 0) result.childNodes.Add(new ParseNode(NodeType.String, piece_start) {
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
                    if (piece.Length > 0) result.childNodes.Add(new ParseNode(NodeType.String, piece_start) {
                        token = state.source.Substring(piece_start, state.start - piece_start) });
                    state.Advance();
                    result.end = state.start;
                    if (result.childNodes.Count == 1 && result.childNodes[0].type == NodeType.String)
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
                if (piece.Length > 0) result.childNodes.Add(new ParseNode(NodeType.String, piece_start)
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
