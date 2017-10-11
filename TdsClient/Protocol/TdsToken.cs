using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TdsClient.Protocol
{
    public abstract class TdsToken
    {
        public TdsToken(TokenType tokenType)
        {
            TokenType = tokenType;
        }

        public TokenType TokenType { get; }

        public virtual bool IsEndOfResponse() => false;

        public override string ToString()
        {
            return $"{TokenType}";
        }

        public virtual string ToShortString()
        {
            return $"{TokenType}";
        }
    }
}
