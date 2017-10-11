using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class ControlToken : TdsToken
    {
        public ControlToken(TokenType tokenType) : base(tokenType)
        {
        }

        public List<string> Formats { get; set; }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append(TokenType);
            s.Append(": [");

            foreach (var f in Formats)
            {
                s.Append($" \"{f}\"");
            }

            s.Append(" ]");
            return s.ToString();
        }
    }
}
