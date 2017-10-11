using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;
using TdsClient.Tokens;

namespace TdsClient
{
    public class TdsDbException : DbException
    {
        public TdsDbException(TextMessageToken serverMessage) : base(serverMessage?.Message, serverMessage?.MessageNumber ?? 0)
        {
            ServerMessage = serverMessage;
        }

        public TextMessageToken ServerMessage { get; }
    }
}
