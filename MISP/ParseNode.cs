using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public enum NodeType
    {
        String,
        StringExpression,
        Token,
        MemberAccess,
        Node,
        Integer,
        DictionaryEntry,
        Count
    }

    public enum Prefix
    {
        Expand,
        Quote,
        List,
        Lookup,
        None,
        Count
    }

    internal static class PrefixCheck
    {
        private static bool[,] allowed = null;
        internal static bool CheckPrefix(ParseNode node)
        {
            if (allowed == null)
            {
                allowed = new bool[(int)NodeType.Count, (int)Prefix.Count];
                for (int i = 0; i < (int)NodeType.Count; ++i) for (int j = 0; j < (int)Prefix.Count; ++j)
                        allowed[i, j] = false;
                for (int i = 0; i < (int)NodeType.Count; ++i) allowed[i, (int)Prefix.None] = true;

                allowed[(int)NodeType.Node, (int)Prefix.Expand] = true;
                allowed[(int)NodeType.Node, (int)Prefix.List] = true;
                allowed[(int)NodeType.Node, (int)Prefix.Quote] = true;
                allowed[(int)NodeType.Node, (int)Prefix.Lookup] = true;

                allowed[(int)NodeType.StringExpression, (int)Prefix.Quote] = true;
                allowed[(int)NodeType.String, (int)Prefix.Quote] = true;
            }

            return allowed[(int)node.type, (int)node.prefix];
        }
    }

    public class ParseNode
    {
        public NodeType type;
        public String token = "";
        public Prefix prefix = Prefix.None;
        public List<ParseNode> childNodes = new List<ParseNode>();
        public int start;
        public int end;
        public ParseState source;
        public int line;

        public ParseNode(NodeType type, int start, ParseState source) 
        { 
            this.type = type; 
            this.start = start;
            this.source = source;
            this.line = source.currentLine;
        }

        public void DebugEmit(int depth)
        {
            Console.WriteLine(new String(' ', depth) + type + ": [" + token + "]");
            foreach (var child in childNodes) child.DebugEmit(depth + 1);
        }
    }

   
}
