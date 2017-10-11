using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class LoginAckToken : TdsToken
    {
        public LoginAckToken(TokenType tokenType) : base(tokenType)
        {
        }

        public bool Succeed { get; internal set; }
        public bool Fail { get; internal set; }
        public bool Negotiate { get; internal set; }
        public string TdsVersion { get; internal set; }
        public string ServerProgram { get; internal set; }
        public string ServerVersion { get; internal set; }

        public override string ToString()
        {
            string status;
            if (Succeed)
            {
                status = "Succeed";
            }
            else if (Fail)
            {
                status = "Fail";
            }
            else if (Negotiate)
            {
                status = "Negotiate";
            }
            else
            {
                status = "Unknown";
            }

            return $"{TokenType}: {{ {status} tds: {TdsVersion}, prog: {ServerProgram} {ServerVersion} }}";
        }

        public override string ToShortString()
        {
            return ToString();
        }
    }
}
