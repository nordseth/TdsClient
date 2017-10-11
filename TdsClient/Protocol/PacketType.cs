using System;
using System.Collections.Generic;
using System.Text;

namespace TdsClient.Protocol
{
    public enum PacketType
    {
        TDS_BUF_LANG = 1, // The buffer contains a language command. TDS does not specify the syntax of the language command.
        TDS_BUF_LOGIN = 2, // The buffer contains a login record
        TDS_BUF_RPC = 3, // The buffer contains a remote procedure call command.
        TDS_BUF_RESPONSE = 4, // The buffer contains the response to a command.
        TDS_BUF_UNFMT = 5, // The buffer contains raw unformatted data.
        TDS_BUF_ATTN = 6, // The buffer contains a non-expedited attention request.
        TDS_BUF_BULK = 7, // The buffer contains bulk binary data.
        TDS_BUF_SETUP = 8, // A protocol request to setup another logical channel. This buffer is a header only and does not contain any data.
        TDS_BUF_CLOSE = 9, // A protocol request to close a logical channel.This buffer is a header only and does not contain any data.
        TDS_BUF_ERROR = 10, // A resource error was detected while attempting to setup or use a logical channel. This buffer is a header only and does not contain any data.
        TDS_BUF_PROTACK = 11, // A protocol acknowledgment associated with the logical channel windowing protocol. This buffer is a header only and does not contain any data.
        TDS_BUF_ECHO = 12, // A protocol request to echo the data contained in the buffer.
        TDS_BUF_LOGOUT = 13, // A protocol request to logout an active logical channel. This buffer is a header only and does not contain any data.
        TDS_BUF_ENDPARAM = 14, // What is this???
        TDS_BUF_NORMAL = 15, // This packet contains a tokenized TDS request or response.
        TDS_BUF_URGENT = 16, // This packet contains an urgent tokenized TDS request or response.
    }
}
