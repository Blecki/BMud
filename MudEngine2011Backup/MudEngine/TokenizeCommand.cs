using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class CommandParser
    {
        internal static void TokenizeCommand(String _command, out String _firstToken, out String _rest)
        {
            _firstToken = "";
            _rest = "";

            int firstSpace = _command.IndexOf(' ');

            if (firstSpace < 0 || firstSpace >= _command.Length)
            {
                _firstToken = _command;
                return;
            }

            _firstToken = _command.Substring(0, firstSpace);

            while (firstSpace < _command.Length && _command[firstSpace] == ' ') ++firstSpace;
            if (firstSpace < _command.Length) _rest = _command.Substring(firstSpace);
        }

        public class Token
        {
            public String Value;
            public int Place;
        }

        internal static LinkedList<Token> FullyTokenizeCommand(String _command)
        {
            var R = new LinkedList<Token>();

            String Buffer = "";
            int BufferStartedAt = 0;

            for (int i = 0; i < _command.Length; ++i)
            {
                if (_command[i] == ' ')
                {
                    if (!String.IsNullOrEmpty(Buffer))
                    {
                        R.AddLast(new Token { Value = Buffer, Place = BufferStartedAt });
                        Buffer = "";
                    }
                    BufferStartedAt = i + 1;
                }
                else
                    Buffer += _command[i];
            }

            if (!String.IsNullOrEmpty(Buffer))
                R.AddLast(new Token { Value = Buffer, Place = BufferStartedAt });

            return R;
        }
    }
}
