using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            string dataArrayString = string.Join(", ", Data.Select(d => $"\"{d}\"").ToArray());
            return $"{TokenType} : [ {dataArrayString} ]";
        }

        public override string ToShortString()
        {
            return $"{TokenType}({Data.Count})";
        }
    }
}
