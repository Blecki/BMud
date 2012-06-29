using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class IRCClientSource
    {
        List<String> ParseMessage(String message)
        {
            var Result = new List<String>();

            String buffer = "";
            for (int i = 0; i < message.Length; ++i)
            {
                if (i == 0 && message[i] == ':')
                { }
                else if (message[i] == ' ')
                {
                    if (!String.IsNullOrEmpty(buffer)) Result.Add(buffer);
                    buffer = "";
                }
                else if (message[i] == ':')
                {
                    if (!String.IsNullOrEmpty(buffer)) Result.Add(buffer);
                    Result.Add(message.Substring(i + 1));
                    i = message.Length;
                }
                else
                    buffer += message[i];
            }

            return Result;
        }

        void ParseHostMask(String Token, out string Nick, out string Hostmask)
        {
            Nick = Token.Substring(0, Token.IndexOf('!'));
            Hostmask = Token.Substring(Token.IndexOf('!') + 1);
        }
    }
}
