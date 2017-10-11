using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class UnknownToken : TdsToken
    {
        public UnknownToken(TokenType tokenType, int len) : base(tokenType)
        {
            Len = len;
        }

        public int Len { get; }
    }
}
