using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class CommandTokenizer
    {
        public class Token : ScriptObject
        {
            public String word;
            public int place;
            public Token next;

            object ScriptObject.GetProperty(string name)
            {
                if (name == "word") return word;
                if (name == "place") return place;
                if (name == "next") return next;
                throw new ScriptError("Token does not have member \"" + name + "\".");
            }

            void ScriptObject.SetProperty(string name, object value)
            {
                throw new ScriptError("Tokens are read-only.");
            }
        }

        internal static Token FullyTokenizeCommand(String _command)
        {
            Token R = null;
            Token current = null;

            String Buffer = "";
            int BufferStartedAt = 0;

            for (int i = 0; i < _command.Length; ++i)
            {
                if (_command[i] == ' ')
                {
                    if (!String.IsNullOrEmpty(Buffer))
                    {
                        var newToken = new Token { word = Buffer, place = BufferStartedAt };
                        if (current != null) current.next = newToken;
                        current = newToken;
                        if (R == null) R = current;
                        Buffer = "";
                    }
                    BufferStartedAt = i + 1;
                }
                else
                    Buffer += _command[i];
            }

            if (!String.IsNullOrEmpty(Buffer))
            {
                var newToken = new Token { word = Buffer, place = BufferStartedAt };
                if (current != null) current.next = newToken;
                current = newToken;
                if (R == null) R = current;
            }

            return R;
        }
    }
}
