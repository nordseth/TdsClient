using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class CapabiltiyToken : TdsToken
    {
        private byte[] _capRequest;
        private byte[] _capResponse;

        public CapabiltiyToken(TokenType tokenType) : base(tokenType)
        {
        }

        public CapabiltiyToken(TokenType tokenType, byte[] capRequest, byte[] capResponse) : base(tokenType)
        {
            _capRequest = capRequest;
            _capResponse = capResponse;
        }

        //todo: parse bit fields and show capabilites
    }
}
