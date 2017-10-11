using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class EnvChangeToken : TdsToken
    {
        public List<Change> Changes { get; internal set; }

        public EnvChangeToken(TokenType tokenType) : base(tokenType)
        {
        }

        public class Change
        {
            public EnvType Type { get; set; }
            public string NewValue { get; set; }
            public string OldValue { get; set; }
        }

        public enum EnvType
        {
            TDS_ENV_DB = 1,
            TDS_ENV_LANG = 2,
            TDS_ENV_CHARSET = 3,
            TDS_ENV_PACKSIZE = 4,
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append(TokenType);
            s.Append(": [");

            foreach (var c in Changes)
            {
                s.Append($" {{ {c.Type} \"{c.OldValue}\" -> \"{c.NewValue}\" }}");
            }

            s.Append(" ]");
            return s.ToString();
        }

        public override string ToShortString()
        {
            return ToString();
        }
    }
}
