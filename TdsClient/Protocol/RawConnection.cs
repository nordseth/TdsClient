using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using TdsClient.Tokens;

namespace TdsClient.Protocol
{
    public class RawConnection : IDisposable
    {
        public const int HeaderLen = 8;
        public const int DefaultPacketLen = 512;

        private ConnectionParamaters _paramaters;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;
        private TokenBuilder _tokenBuilder;
        private Dictionary<Type, Action<TdsToken>> _tokenHandlers;

        private Socket _socket;
        private TdsRequestStream _req;
        private TdsResponseStream _res;

        public int PacketSize { get; private set; } = DefaultPacketLen;
        public Encoding Encoder { get; private set; } = Encoding.ASCII;
        public string Language { get; private set; }
        public string Database { get; private set; }

        public List<TextMessageToken> ResponseMessages { get; private set; } = new List<TextMessageToken>();
        public bool ResponseIsError { get; private set; }
        public bool ResponseAtEnd { get; private set; }
        public bool InCancel { get; private set; }

        public RawConnection(ConnectionParamaters connectionParamaters, ILoggerFactory loggerFactory)
        {
            _paramaters = connectionParamaters;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger<RawConnection>();
            _tokenBuilder = new TokenBuilder(_loggerFactory?.CreateLogger<TokenBuilder>());

            _tokenHandlers = new Dictionary<Type, Action<TdsToken>>
            {
                [typeof(EnvChangeToken)] = EnvChangeRecived,
                [typeof(TextMessageToken)] = TextMsgRecived,
            };
        }

        public void Connect()
        {
            System.Net.IPEndPoint endPoint;
            if (System.Net.IPAddress.TryParse(_paramaters.ServerAddress, out var ipadr))
            {
                endPoint = new System.Net.IPEndPoint(ipadr, _paramaters.ServerPort);
            }
            else
            {
                var ip = System.Net.Dns.GetHostEntryAsync(_paramaters.ServerAddress).GetAwaiter().GetResult();
                endPoint = new System.Net.IPEndPoint(
                    ip.AddressList.OrderByDescending(a => a.AddressFamily == AddressFamily.InterNetwork).First(),
                    _paramaters.ServerPort);
            }

            _logger?.LogDebug($"Connecting to {_paramaters.ServerAddress} -> {endPoint}");
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
            _socket.Connect(endPoint);

            _req = new TdsRequestStream(_socket, this, _loggerFactory?.CreateLogger<TdsRequestStream>());
            _res = new TdsResponseStream(_socket, this, _loggerFactory?.CreateLogger<TdsResponseStream>());
        }

        public void SendLogin()
        {
            StartRequest();
            _logger?.LogDebug("Sending login message");

            _req.SetPacketType(PacketType.TDS_BUF_LOGIN);
            _req.WriteLoginString(_paramaters.ClientHostname, 30);// client hostname
            _req.WriteLoginString(_paramaters.Username, 30); // client username
            _req.WriteLoginString(_paramaters.Password, 30); // client password
            _req.WriteLoginString(_paramaters.ProcessId, 30); // client process name

            _req.Write((byte)3); // type of int2
            _req.Write((byte)1); // type of int4
            _req.Write((byte)6); // type of char
            _req.Write((byte)10);// type of flt
            _req.Write((byte)9); // type of date
            _req.Write((byte)1); // notify of use db
            _req.Write((byte)1); // disallow dump/load and bulk insert
            _req.Write((byte)0); // sql interface type
            _req.Write((byte)0); // type of network connection

            _req.Write(null, 0, 7);

            _req.WriteLoginString(_paramaters.AppliactionName, 30); // client application name
            _req.WriteLoginString(_paramaters.ServerName, 30); // server name
            _req.Write((byte)0); // remote passwords
            _req.Write((byte)_paramaters.Password.Length);
            byte[] tmpPassword = Encoder.GetBytes(_paramaters.Password);
            _req.Write(tmpPassword, 0, 253);
            _req.Write((byte)(tmpPassword.Length + 2));

            _req.Write((byte)5);  // tds version
            _req.Write((byte)0);

            _req.Write((byte)0);
            _req.Write((byte)0);
            _req.WriteLoginString("TdsClient", 10); // client library

            _req.Write((byte)5);  // prog version
            _req.Write((byte)0);
            _req.Write((byte)0);
            _req.Write((byte)0);

            _req.Write((byte)0);  // auto convert short
            _req.Write((byte)0x0D); // type of flt4
            _req.Write((byte)0x11); // type of date4

            _req.WriteLoginString(_paramaters.Language, 30);  // language

            _req.Write((byte)1);  // notify on lang change
            _req.Write((byte)0);  // security label hierachy 1
            _req.Write((byte)0);  // security label hierachy 2
            _req.Write((byte)0);  // security encrypted 
            _req.Write(null, 0, 8);  // security components
            _req.Write((byte)0);  // security spare 1
            _req.Write((byte)0);  // security spare 2

            _req.WriteLoginString(_paramaters.Charset, 30); // Character set

            _req.Write((byte)1);  // notify on charset change

            _req.WriteLoginString(PacketSize.ToString(), 6);
            _req.Write(null, 0, 4);

            // send capability token?

            _req.EndMessage();
        }

        public void ReadLoginResponse()
        {
            var tokens = GetResponse();

            var loginAck = tokens.OfType<LoginAckToken>().FirstOrDefault();
            if (loginAck != null)
            {
                if (!loginAck.Succeed)
                {
                    throw new Exception("Login failed");
                }
            }
            else
            {
                throw new Exception("No login ack from server");
            }
        }

        public void ExecuteSql(string sql, IEnumerable<TdsParameter> parameters)
        {
            StartRequest();
            _logger?.LogInformation($"Sending language(sql) message with {parameters?.Count()} parameters: \"{sql}\"");

            _req.SetPacketType(PacketType.TDS_BUF_NORMAL);
            _req.Write((byte)TokenType.TDS_LANGUAGE);

            var sqlBytes = Encoder.GetBytes(sql);
            _req.WriteInt(sqlBytes.Length + 1);
            _req.Write(parameters != null && parameters.Any() ? (byte)1 : (byte)0);
            _req.Write(sqlBytes, 0, sql.Length);

            if (parameters != null && parameters.Any())
            {
                WriteParameters(parameters);
            }

            _req.EndMessage();
        }

        public IEnumerable<TdsToken> GetResponse()
        {
            var tokens = GetResponseStream().ToList();

            if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                var s = new StringBuilder();
                s.AppendLine($"Read response with {tokens.Count} tokens:");
                foreach (var t in tokens)
                {
                    s.AppendLine(t.ToShortString());
                }

                _logger.LogDebug(s.ToString());
            }

            return tokens;
        }

        public IEnumerable<TdsToken> GetResponseStream()
        {
            FormatToken format = null;
            while (true)
            {
                if (!ResponseAtEnd)
                {
                    TdsToken token = _tokenBuilder.ReadToken(_res, Encoder, format);

                    if (_tokenHandlers.TryGetValue(token.GetType(), out var handler))
                    {
                        handler(token);
                    }

                    if (token is FormatToken)
                    {
                        format = (FormatToken)token;
                    }

                    ResponseAtEnd = token.IsEndOfResponse();
                    yield return token;
                }
                else
                {
                    yield break;
                }
            }
        }

        public void SendCancel()
        {
            InCancel = true;
            _logger?.LogInformation("Sending cancel");
            _req.SetPacketType(PacketType.TDS_BUF_ATTN);
            _req.EndMessage();
        }

        public void WaitForCancel()
        {
            _logger?.LogInformation("Reading and discarding from cancel");
            throw new NotImplementedException();
            //var tokens = this.ToList();
        }

        public void Dispose()
        {
            _socket.Dispose();
            _socket = null;
        }

        private void WriteParameters(IEnumerable<TdsParameter> parameters)
        {
            _req.Write((byte)TokenType.TDS_PARAMFMT);

            int len = 2;
            len += parameters
                .Select(p => TypeReader.GetFormatLen(p.TdsType, p.ParameterName, Encoder))
                .Sum();

            _req.WriteShort(len);
            _req.WriteShort(parameters.Count());

            foreach (var p in parameters)
            {
                TypeReader.WriteFormat(p, _req, Encoder);
            }

            _req.Write((byte)TokenType.TDS_PARAMS);

            foreach (var p in parameters)
            {
                TypeReader.WriteData(p, _req, Encoder);
            }
        }

        private void EnvChangeRecived(TdsToken token)
        {
            var change = (EnvChangeToken)token;
            foreach (var c in change.Changes)
            {
                switch (c.Type)
                {
                    case EnvChangeToken.EnvType.TDS_ENV_DB:
                        Database = c.NewValue;
                        _logger?.LogInformation($"Database changed from {c.OldValue} to {c.NewValue}");
                        break;
                    case EnvChangeToken.EnvType.TDS_ENV_LANG:
                        Language = c.NewValue;
                        _logger?.LogInformation($"Language changed from {c.OldValue} to {c.NewValue}");
                        break;
                    case EnvChangeToken.EnvType.TDS_ENV_CHARSET:
                        Encoder = GetEncoder(c.NewValue);
                        _logger?.LogInformation($"Charset changed from {c.OldValue} to {c.NewValue} ({Encoder.WebName})");
                        break;
                    case EnvChangeToken.EnvType.TDS_ENV_PACKSIZE:
                        PacketSize = int.Parse(c.NewValue);
                        _logger?.LogInformation($"Packet size changed from {c.OldValue} to {c.NewValue}");
                        break;
                    default:
                        _logger?.LogWarning($"Unknown env change {c.Type} from {c.OldValue} to {c.NewValue}");
                        break;
                }
            }
        }

        private Encoding GetEncoder(string charset)
        {
            if (charset.Equals("iso_1", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.GetEncoding("ISO-8859-1");
            }
            else if (charset.Equals("UTF8", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF8;
            }
            else if (charset.Equals("UNICODE", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.Unicode;
            }
            else if (charset.Equals("ASCII_8", StringComparison.OrdinalIgnoreCase)
                || charset.Equals("ASCII_7", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.ASCII;
            }
            else
            {
                _logger?.LogError($"Unknown charset of type {charset}");
                throw new Exception($"Unknown charset of type {charset}");
            }
        }

        private void TextMsgRecived(TdsToken token)
        {
            var msg = (TextMessageToken)token;
            if (msg.Severity > 10)
            {
                _logger?.LogError($"Server error: {msg.GetMessage()}");
                ResponseMessages.Add(msg);
                ResponseIsError = true;
            }
            else
            {
                _logger?.LogInformation($"Server info: {msg.GetMessage()}");
                ResponseMessages.Add(msg);
            }
        }

        private void StartRequest()
        {
            ResponseMessages.Clear();
            ResponseIsError = false;
            ResponseAtEnd = false;
        }
    }
}
