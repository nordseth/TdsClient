using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TdsClient.Protocol
{
    public class TdsResponseStream
    {
        private Socket _socket;
        private RawConnection _conn;
        private ILogger _logger;

        private byte[] _buffer;
        private int _position;
        private int _packetLen;
        private PacketType _packetType;

        public TdsResponseStream(Socket socket, RawConnection conn, ILogger logger)
        {
            _socket = socket;
            _conn = conn;
            _logger = logger;

            _buffer = new byte[_conn.PacketSize];
            _position = 0;
            _packetLen = 0;
        }

        public byte Read()
        {
            if (_position >= _packetLen)
            {
                ReadPacket();
            }

            return _buffer[_position++];
        }

        public int Read(byte[] b, int off, int len)
        {
            int bytesToRead = len;

            while (bytesToRead > 0)
            {
                if (_position >= _packetLen)
                {
                    ReadPacket();
                }

                int available = _packetLen - _position;
                int bc = (available > bytesToRead) ? bytesToRead : available;

                Array.Copy(_buffer, _position, b, off, bc);
                off += bc;
                bytesToRead -= bc;
                _position += bc;
            }

            return len;
        }

        public byte[] ReadBytes(int len)
        {
            var b = new byte[len];
            Read(b, 0, len);
            return b;
        }

        public byte[] ReadBytes(int len, int padToLen)
        {
            var b = new byte[Math.Max(len, padToLen)];
            Read(b, 0, len);
            return b;
        }

        public string ReadString(int len)
        {
            if (len == 0)
            {
                return null;
            }

            var b = ReadBytes(len);
            return _conn.Encoder.GetString(b);
        }

        public short ReadShort()
        {
            int b1 = Read();

            return (short)(b1 | (Read() << 8));
        }

        public int ReadInt()
        {
            int b1 = Read();
            int b2 = Read() << 8;
            int b3 = Read() << 16;
            int b4 = Read() << 24;

            return b4 | b3 | b2 | b1;
        }

        public long ReadLong()
        {
            long b1 = Read();
            long b2 = ((long)Read()) << 8;
            long b3 = ((long)Read()) << 16;
            long b4 = ((long)Read()) << 24;
            long b5 = ((long)Read()) << 32;
            long b6 = ((long)Read()) << 40;
            long b7 = ((long)Read()) << 48;
            long b8 = ((long)Read()) << 56;

            return b1 | b2 | b3 | b4 | b5 | b6 | b7 | b8;
        }

        public int SkipRead(int skip)
        {
            int tmp = skip;

            while (skip > 0)
            {
                if (_position >= _packetLen)
                {
                    ReadPacket();
                }

                int available = _packetLen - _position;

                if (skip > available)
                {
                    skip -= available;
                    _position = _packetLen;
                }
                else
                {
                    _position += skip;
                    skip = 0;
                }
            }

            return tmp;
        }

        private void ReadPacket()
        {
            //_logger?.LogTrace($"ReadPacket....");
            ReceiveExactly(_buffer, 0, RawConnection.HeaderLen);

            _packetType = (PacketType)_buffer[0];

            if (_packetType != PacketType.TDS_BUF_LOGIN
                    && _packetType != PacketType.TDS_BUF_LANG
                    && _packetType != PacketType.TDS_BUF_NORMAL
                    && _packetType != PacketType.TDS_BUF_RESPONSE)
            {
                throw new Exception($"Unknown packet type {_packetType}");
            }

            _packetLen = GetPacketLen(_buffer);
            _logger?.LogTrace($"read {_packetType} of size {_packetLen}, status: {_buffer[1]} ");

            if (_packetLen < RawConnection.HeaderLen || _packetLen > 65536)
            {
                throw new Exception($"Invalid network packet length {_packetLen}");
            }

            if (_packetLen > _buffer.Length)
            {
                var oldBuffer = _buffer;
                _buffer = new byte[_packetLen];
                Array.Copy(oldBuffer, 0, _buffer, 0, RawConnection.HeaderLen);
            }

            ReceiveExactly(_buffer, RawConnection.HeaderLen, _packetLen - RawConnection.HeaderLen);
            _position = RawConnection.HeaderLen;
        }

        private void ReceiveExactly(byte[] buffer, int offset, int length)
        {
            var receivedLength = 0;
            while (receivedLength < length)
            {
                int nextLength = _socket.Receive(buffer, receivedLength + offset, length - receivedLength, SocketFlags.None);
                if (nextLength == 0)
                {
                    throw new Exception("Socket closed");
                }
                receivedLength += nextLength;
            }
        }

        private static int GetPacketLen(byte[] b)
        {
            int lo = (b[3] & 0xff);
            int hi = ((b[2] & 0xff) << 8);

            return hi | lo;
        }
    }
}
