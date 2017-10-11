using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class DataToken : TdsToken
    {
        public DataToken(TokenType tokenType) : base(tokenType)
        {
        }

        public List<object> Data { get; internal set; }

        public override string ToString()
        {
            var jsonData = JsonConvert.SerializeObject(Data);
            return $"{TokenType} : {jsonData}";
        }

        public override string ToShortString()
        {
            return $"{TokenType}({Data.Count})";
        }
    }
}
