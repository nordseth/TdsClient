using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class ReturnStatusToken : TdsToken
    {
        public ReturnStatusToken(TokenType tokenType) : base(tokenType)
        {
        }

        public int Value { get; internal set; }

        public override string ToString()
        {
            return $"{TokenType}: {{ {Value} }}";
        }

        public override string ToShortString()
        {
            return ToString();
        }
    }
}
