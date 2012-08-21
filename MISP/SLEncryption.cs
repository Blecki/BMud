using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace MISP
{
    public partial class Engine
    {
        SHA512CryptoServiceProvider hash = new SHA512CryptoServiceProvider();
        ASCIIEncoding byteEncoding = new ASCIIEncoding();

        private void SetupEncryptionFunctions()
        {
            functions.Add("hash", new Function("hash",
                ArgumentInfo.ParseArguments(this, "string value", "string salt"),
                "string: Hashes the string.",
                (context, arguments) =>
                {
                    var encodedString = byteEncoding.GetBytes(ScriptObject.AsString(arguments[0])
                        + ScriptObject.AsString(arguments[1]));
                    var hashed = hash.ComputeHash(encodedString);
                    return Convert.ToBase64String(hashed);
                }));
        }
    }
}
