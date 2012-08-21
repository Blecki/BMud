using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class ParseState
    {
        public int start;
        public int end;
        public String source;
        public int currentLine = 1;
        public String filename;

        public char Next()
        {
            if (start >= source.Length)
                throw new ScriptError("Parser error.", null);
            return source[start]; 
        }

        public void Advance(int distance = 1) 
        {
            while (distance > 0)
            {
                start += 1;
                distance -= 1;
                if (start > end) throw new ParseError("Unexpected end of script", currentLine);
                if (!AtEnd() && Next() == '\n') currentLine += 1;
            }
        }
        
        public bool AtEnd() { return start == end; }

        public bool MatchNext(String str) 
        {
            if (str.Length + start > source.Length) return false;
            return str == source.Substring(start, str.Length); 
        }
    }
}
