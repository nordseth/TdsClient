using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class TextMessageToken : TdsToken
    {
        public TextMessageToken(TokenType tokenType) : base(tokenType)
        {
        }

        public int MessageNumber { get; internal set; }
        public int State { get; internal set; }
        public int Severity { get; internal set; }
        public bool ParamsFollows { get; internal set; }
        public string Message { get; internal set; }
        public string ServerName { get; internal set; }
        public string ProcName { get; internal set; }
        public int LineNumber { get; internal set; }

        public override string ToString()
        {
            return $"{TokenType}: {{ {(Severity > 10 ? "Error" : "Info")}: {GetMessage()} }}";
        }

        public string GetMessage()
        {
            var msg = JsonConvert.SerializeObject(Message);
            if (!string.IsNullOrEmpty(ProcName))
            {
                return $"{MessageNumber}/{State}: {msg} on {ServerName} in {ProcName}:{LineNumber}";
            }
            else
            {
                return $"{MessageNumber}/{State}: {msg} on {ServerName}";
            }
        }

        public override string ToShortString()
        {
            return ToString();
        }
    }
}
