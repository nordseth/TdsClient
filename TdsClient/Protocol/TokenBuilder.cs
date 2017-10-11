using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TdsClient.Tokens;

namespace TdsClient.Protocol
{
    public class TokenBuilder
    {
        private readonly Dictionary<TokenType, TokenDescriptor> _tokenDescriptors;
        private ILogger _logger;

        public TokenBuilder(ILogger logger)
        {
            _logger = logger;
            var descriptors = new[]
            {
                new TokenDescriptor(TokenType.TDS_MSG, TokenLength.OneByteLen, null),

                new TokenDescriptor(TokenType.TDS_CAPABILITY, TokenLength.TwoByteLen, CreateCapabiltiy),
                new TokenDescriptor(TokenType.TDS_DBRPC, TokenLength.TwoByteLen, null),
                new TokenDescriptor(TokenType.TDS_DYNAMIC, TokenLength.TwoByteLen, null),
                new TokenDescriptor(TokenType.TDS_EED, TokenLength.TwoByteLen, CreateTextMessage),
                new TokenDescriptor(TokenType.TDS_ENVCHANGE, TokenLength.TwoByteLen, CreateEnvChange),
                new TokenDescriptor(TokenType.TDS_LOGINACK, TokenLength.TwoByteLen, CreateLoginAck),
                new TokenDescriptor(TokenType.TDS_PARAMFMT, TokenLength.TwoByteLen, CreateFormat),
                new TokenDescriptor(TokenType.TDS_ROWFMT, TokenLength.TwoByteLen, CreateFormat),
                new TokenDescriptor(TokenType.TDS_CONTROL, TokenLength.TwoByteLen, CreateControl),
                new TokenDescriptor(TokenType.TDS_ORDERBY, TokenLength.TwoByteLen, null),

                new TokenDescriptor(TokenType.TDS_DYNAMIC2, TokenLength.FourByteLen, null),
                new TokenDescriptor(TokenType.TDS_LANGUAGE, TokenLength.FourByteLen, null),
                new TokenDescriptor(TokenType.TDS_ORDERBY2, TokenLength.FourByteLen, null),
                new TokenDescriptor(TokenType.TDS_PARAMFMT2, TokenLength.FourByteLen, CreateFormat),
                new TokenDescriptor(TokenType.TDS_ROWFMT2, TokenLength.FourByteLen, CreateFormat),

                new TokenDescriptor(TokenType.TDS_LOGOUT, TokenLength.OneByte, null),

                new TokenDescriptor(TokenType.TDS_RETURNSTATUS, TokenLength.FourBytes, CreateReturnStatus),

                new TokenDescriptor(TokenType.TDS_DONE, TokenLength.EigthBytes, CreateDone),
                new TokenDescriptor(TokenType.TDS_DONEINPROC, TokenLength.EigthBytes, CreateDone),
                new TokenDescriptor(TokenType.TDS_DONEPROC, TokenLength.EigthBytes, CreateDone),

                new TokenDescriptor(TokenType.TDS_KEY, TokenLength.Dependent, null),
                new TokenDescriptor(TokenType.TDS_PARAMS, TokenLength.Dependent, null),
                new TokenDescriptor(TokenType.TDS_ROW, TokenLength.Dependent, CreateData),
            };

            _tokenDescriptors = descriptors.ToDictionary(k => k.TokenType, v => v);
        }

        public TdsToken ReadToken(TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            var tokenType = (TokenType)stream.Read();

            TdsToken token = null;
            TokenDescriptor desc;
            if (_tokenDescriptors.TryGetValue(tokenType, out desc))
            {
                if (desc.Builder == null)
                {
                    token = CreateNotImplementedToken(desc, stream);
                }
                else
                {
                    //_logger?.LogTrace($"Building token of type {tokenType}");
                    token = desc.Builder(desc, stream, encoder, lastFormat);
                    _logger?.LogTrace(token.ToString());
                }
            }
            else
            {
                _logger?.LogError($"Unknown token of type {tokenType}");
                throw new Exception($"Unknown token of type {tokenType}");
            }

            return token;
        }

        private TdsToken CreateNotImplementedToken(TokenDescriptor tokenDesc, TdsResponseStream stream)
        {
            int len = 0;
            switch (tokenDesc.Len)
            {
                case TokenLength.OneByteLen:
                    len = stream.Read();
                    stream.SkipRead(len);
                    len += 1;
                    break;
                case TokenLength.TwoByteLen:
                    len = stream.ReadShort();
                    stream.SkipRead(len);
                    len += 2;
                    break;
                case TokenLength.FourByteLen:
                    len = stream.ReadInt();
                    stream.SkipRead(len);
                    len += 4;
                    break;
                case TokenLength.OneByte:
                    len = 1;
                    stream.SkipRead(len);
                    break;
                case TokenLength.FourBytes:
                    len = 4;
                    stream.SkipRead(len);
                    break;
                case TokenLength.EigthBytes:
                    len = 8;
                    stream.SkipRead(len);
                    break;
                case TokenLength.Dependent:
                default:
                    _logger?.LogError($"Unknown length of token of type {tokenDesc.TokenType}");
                    throw new Exception($"Unknown length of token of type {tokenDesc.TokenType}");
            }

            _logger?.LogWarning($"Not implemented token {tokenDesc.TokenType} of len {len}");
            return new UnknownToken(tokenDesc.TokenType, len);
        }

        private TdsToken CreateCapabiltiy(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int len = stream.ReadShort();

            if (stream.Read() != 1)
            {
                throw new Exception("TDS_CAPABILITY: expected request string");
            }
            int capReqLen = stream.Read();
            if (capReqLen != 11 && capReqLen != 0)
            {
                throw new Exception("TDS_CAPABILITY: byte count not 11");
            }

            byte[] capRequest = new byte[11];
            if (capReqLen != 0)
            {
                stream.Read(capRequest, 0, 11);
            }

            if (stream.Read() != 2)
            {
                throw new Exception("TDS_CAPABILITY: expected response string");
            }

            int capResLen = stream.Read();
            if (capResLen != 10 && capResLen != 0)
            {
                throw new Exception("TDS_CAPABILITY: byte count not 10");
            }

            byte[] capResponse = new byte[10];
            if (capResLen != 0)
            {
                stream.Read(capResponse, 0, 10);
            }

            return new CapabiltiyToken(tokenDesc.TokenType, capRequest, capResponse);
        }

        private TdsToken CreateTextMessage(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int len = stream.ReadShort();
            int msgNumber = stream.ReadInt();
            int state = stream.Read();
            int severity = stream.Read();

            int sqlStatusLen = stream.Read();
            var sqlStatus = stream.ReadBytes(sqlStatusLen);

            int status = stream.Read();
            int tranState = stream.ReadShort(); // discarded

            int msgLen = stream.ReadShort();
            var msg = stream.ReadString(msgLen);

            int srvNameLen = stream.Read();
            var serverName = stream.ReadString(srvNameLen);

            int procNameLen = stream.Read();
            var procName = stream.ReadString(procNameLen);

            int lineNumber = stream.ReadShort();

            return new TextMessageToken(tokenDesc.TokenType)
            {
                MessageNumber = msgNumber,
                State = state,
                Severity = severity,
                ParamsFollows = status == 1,
                Message = msg,
                ServerName = serverName,
                ProcName = procName,
                LineNumber = lineNumber,
            };
        }

        private TdsToken CreateDone(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int status = stream.ReadShort();
            int tranState = stream.ReadShort();
            int count = stream.ReadInt();

            return new DoneToken(tokenDesc.TokenType)
            {
                More = (status & 1) != 0,
                Error = (status & 2) != 0,
                InTransaction = (status & 4) != 0,
                Proc = (status & 8) != 0,
                Count = (status & 16) != 0,
                AttentionAck = (status & 32) != 0,
                //Event = (status & 64) != 0,

                TransactionState = (DoneToken.TransactionStatus)tranState,

                RowCount = (status & 16) != 0 ? count : (int?)null,
            };
        }

        private TdsToken CreateLoginAck(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int len = stream.ReadShort();
            int status = stream.Read();
            var version = stream.ReadBytes(4);
            int nameLen = stream.Read();
            var name = stream.ReadString(nameLen);
            var progVersion = stream.ReadBytes(4);

            return new LoginAckToken(tokenDesc.TokenType)
            {
                Succeed = status == 5,
                Fail = status == 6,
                Negotiate = status == 7,
                TdsVersion = $"{version[0]}.{version[1]}.{version[2]}.{version[3]}",
                ServerProgram = name,
                ServerVersion = $"{progVersion[0]}.{progVersion[1]}.{progVersion[2]}.{progVersion[3]}",
            };
        }

        private TdsToken CreateEnvChange(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int len = stream.ReadShort();
            int read = 0;

            var changes = new List<EnvChangeToken.Change>();
            while (read < len)
            {
                var change = new EnvChangeToken.Change();
                change.Type = (EnvChangeToken.EnvType)stream.Read();
                int newValLen = stream.Read();
                change.NewValue = stream.ReadString(newValLen);
                int oldValLen = stream.Read();
                change.OldValue = stream.ReadString(oldValLen);
                read += 3 + newValLen + oldValLen;

                changes.Add(change);
            }

            return new EnvChangeToken(tokenDesc.TokenType)
            {
                Changes = changes
            };
        }

        private TdsToken CreateFormat(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int len;
            if (tokenDesc.Len == TokenLength.FourByteLen)
            {
                len = stream.ReadInt();
            }
            else
            {
                len = stream.ReadShort();
            }

            int numFormats = stream.ReadShort();

            var formats = new List<FormatToken.Format>();

            for (int i = 0; i < numFormats; i++)
            {
                var f = new FormatToken.Format
                {
                    Params = tokenDesc.TokenType == TokenType.TDS_PARAMFMT || tokenDesc.TokenType == TokenType.TDS_PARAMFMT2,
                };

                int nameLen = stream.Read();
                f.Name = stream.ReadString(nameLen);

                if (tokenDesc.TokenType == TokenType.TDS_ROWFMT2)
                {
                    int catalogNameLen = stream.Read();
                    f.CatalogName = stream.ReadString(catalogNameLen);
                    int schemaNameLen = stream.Read();
                    f.SchemaName = stream.ReadString(schemaNameLen);
                    int tableNameLen = stream.Read();
                    f.TableName = stream.ReadString(tableNameLen);
                    int columnNameLen = stream.Read();
                    f.ColumnName = stream.ReadString(columnNameLen);
                }

                if (tokenDesc.Len == TokenLength.FourByteLen)
                {
                    f.Status = stream.ReadInt();
                }
                else
                {
                    f.Status = stream.Read();
                }

                f.UserType = (UserDataType)stream.ReadInt();
                f.Type = (TdsType)stream.Read();

                TypeReader.ReadFormat(f, stream);

                formats.Add(f);
            }

            return new FormatToken(tokenDesc.TokenType)
            {
                Formats = formats,
            };
        }

        private TdsToken CreateControl(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int len = stream.ReadShort();
            int read = 0;

            var formats = new List<string>();
            while (read < len)
            {
                int formatLen = stream.Read();
                var format = stream.ReadString(formatLen);
                read += 1 + formatLen;

                formats.Add(format);
            }

            return new ControlToken(tokenDesc.TokenType)
            {
                Formats = formats
            };
        }

        private TdsToken CreateData(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            if (lastFormat == null)
            {
                _logger?.LogError($"Format for {tokenDesc.TokenType} not found");
                throw new Exception($"Format for {tokenDesc.TokenType} not found");
            }

            var data = new List<object>();

            foreach (var f in lastFormat.Formats)
            {
                data.Add(TypeReader.ReadData(f, stream, encoder));
            }

            return new DataToken(tokenDesc.TokenType)
            {
                Data = data,
            };
        }

        private TdsToken CreateReturnStatus(TokenDescriptor tokenDesc, TdsResponseStream stream, Encoding encoder, FormatToken lastFormat)
        {
            int value = stream.ReadInt();

            return new ReturnStatusToken(tokenDesc.TokenType)
            {
                Value = value,
            };
        }

        private class TokenDescriptor
        {
            public TokenDescriptor(TokenType type, TokenLength len, Func<TokenDescriptor, TdsResponseStream, Encoding, FormatToken, TdsToken> builder)
            {
                TokenType = type;
                Len = len;
                Builder = builder;
            }

            public TokenType TokenType { get; }
            public TokenLength Len { get; }
            public Func<TokenDescriptor, TdsResponseStream, Encoding, FormatToken, TdsToken> Builder { get; }
        }

        private enum TokenLength
        {
            OneByteLen,
            TwoByteLen,
            FourByteLen,
            OneByte,
            FourBytes,
            EigthBytes,
            Dependent,
        }
    }
}
