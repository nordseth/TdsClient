using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TdsClient.Protocol
{
    public class TdsRequestStream
    {
        private RawConnection _conn;
        private Socket _socket;
        private ILogger _logger;

        private byte[] _buffer;
        private int _position;
        private PacketType _packetType;

        public TdsRequestStream(Socket socket, RawConnection conn, ILogger logger)
        {
            _conn = conn;
            _socket = socket;
            _logger = logger;
            _buffer = new byte[_conn.PacketSize];
            _position = RawConnection.HeaderLen;
        }

        public void SetPacketType(PacketType type)
        {
            _packetType = type;
        }

        public void WriteLoginString(string s, int maxLen)
        {
            var bytes = _conn.Encoder.GetBytes(s);
            int len = Math.Min(maxLen, bytes.Length);
            Write(bytes, 0, maxLen);
            Write((byte)len);
        }

        public void Write(byte b)
        {
            if (_position == _buffer.Length)
            {
                SendPacket(false);
            }

            _buffer[_position++] = b;
        }

        public void WriteShort(int s)
        {
            Write((byte)s);
            Write((byte)(s >> 8));
        }

        public void WriteInt(int i)
        {
            Write((byte)i);
            Write((byte)(i >> 8));
            Write((byte)(i >> 16));
            Write((byte)(i >> 24));
        }

        public void WriteLong(long l)
        {
            Write((byte)l);
            Write((byte)(l >> 8));
            Write((byte)(l >> 16));
            Write((byte)(l >> 24));
            Write((byte)(l >> 32));
            Write((byte)(l >> 40));
            Write((byte)(l >> 48));
            Write((byte)(l >> 56));
        }

        public void Write(byte[] b)
        {
            Write(b, 0, b.Length);
        }

        public void Write(byte[] b, int off, int len)
        {
            // allowed with empty and null array
            // pads to len with 0

            int bLen = (b?.Length ?? 0);
            int limit = (off + len) > bLen ? bLen : off + len;
            int bytesToWrite = limit - off;
            int i = len - bytesToWrite;

            while (bytesToWrite > 0)
            {
                int available = _buffer.Length - _position;

                if (available == 0)
                {
                    SendPacket(false);
                }
                else
                {
                    int bc = (available > bytesToWrite) ? bytesToWrite : available;
                    Array.Copy(b, off, _buffer, _position, bc);
                    off += bc;
                    _position += bc;
                    bytesToWrite -= bc;
                }
            }

            for (; i > 0; i--)
            {
                Write(0);
            }
        }

        public void EndMessage()
        {
            SendPacket(true);
        }

        private void SendPacket(bool last)
        {
            _buffer[0] = (byte)_packetType;
            _buffer[1] = last ? (byte)1 : (byte)0; // last segment indicator
            _buffer[2] = (byte)(_position >> 8);
            _buffer[3] = (byte)_position;
            _buffer[4] = 0;
            _buffer[5] = 0;
            _buffer[6] = 0;
            _buffer[7] = 0;

            _logger?.LogTrace($"SendPacket({last}) type: {_packetType}, len {_position}");

            _socket.Send(_buffer, 0, _position, SocketFlags.None);

            if (_conn.PacketSize > _buffer.Length)
            {
                var oldBuffer = _buffer;
                _buffer = new byte[_conn.PacketSize];
                Array.Copy(oldBuffer, 0, _buffer, 0, RawConnection.HeaderLen);
            }

            _position = RawConnection.HeaderLen;
        }
    }
}
