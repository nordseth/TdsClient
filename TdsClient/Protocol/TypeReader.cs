using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using TdsClient.Tokens;

namespace TdsClient.Protocol
{
    public static class TypeReader
    {
        private static readonly byte VAR_MAX = 255;

        private static readonly Dictionary<TdsType, TypeInfo> _types;

        static TypeReader()
        {
            var types = new[]
            {
                new TypeInfo(TdsType.TDS_BIT,       true, false, false, FormatFormat.Empty, DataFormat.OneByte), // "Bit", format: (empty), data: data(1)
                new TypeInfo(TdsType.TDS_FLT4,      true, false, true, FormatFormat.Empty, DataFormat.FourBytes), // "Float", format: (empty), data: data(4)
                new TypeInfo(TdsType.TDS_FLT8,      true, false, true, FormatFormat.Empty, DataFormat.EightBytes), // "Float", format: (empty), data: data(8)
                new TypeInfo(TdsType.TDS_INT1,      true, false, false, FormatFormat.Empty, DataFormat.OneByte), // "Integer", format: (empty), data: data(1)
                new TypeInfo(TdsType.TDS_INT2,      true, false, true, FormatFormat.Empty, DataFormat.TwoBytes), // "Integer", format: (empty), data: data(2)
                new TypeInfo(TdsType.TDS_INT4,      true, false, true, FormatFormat.Empty, DataFormat.FourBytes), // "Integer", format: (empty), data: data(4)
                new TypeInfo(TdsType.TDS_INT8,      true, false, true, FormatFormat.Empty, DataFormat.EightBytes), // "Integer", format: (empty), data: data(8)
                new TypeInfo(TdsType.TDS_UINT1,     true, false, false, FormatFormat.Empty, DataFormat.OneByte), // "Unsigned Integer", format: (empty), data: data(1)
                new TypeInfo(TdsType.TDS_UINT2,     true, false, true, FormatFormat.Empty, DataFormat.TwoBytes), // "Unsigned Integer", format: (empty), data: data(2)
                new TypeInfo(TdsType.TDS_UINT4,     true, false, true, FormatFormat.Empty, DataFormat.FourBytes), // "Unsigned Integer", format: (empty), data: data(4)
                new TypeInfo(TdsType.TDS_UINT8,     true, false, true, FormatFormat.Empty, DataFormat.EightBytes), // "Unsigned Integer", format: (empty), data: data(8)
                new TypeInfo(TdsType.TDS_MONEY,     true, false, true, FormatFormat.Empty, DataFormat.EightBytes), // "Money", format: (empty), data: data(8)
                new TypeInfo(TdsType.TDS_SHORTMONEY,true, false, true, FormatFormat.Empty, DataFormat.FourBytes), // "Money", format: (empty), data: data(4)
                new TypeInfo(TdsType.TDS_DATETIME,  true, false, true, FormatFormat.Empty, DataFormat.EightBytes), // "Date/time", format: (empty), data: data(8)
                new TypeInfo(TdsType.TDS_SHORTDATE, true, false, true, FormatFormat.Empty, DataFormat.FourBytes), // "Date/time", format: (empty), data: data(4)

                new TypeInfo(TdsType.TDS_BINARY,    true, false, false, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Binary",  format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_CHAR,      true, false, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Character", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_DATETIMEN, false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Date/time", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_FLTN,      false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Float",  format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_INTN,      false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Integer",  format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_UINTN,     false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Unsigned Integer",  format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_MONEYN,    false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Money", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_VARBINARY, false, true, false, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Binary", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_SENSITIVITY,false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Sensitivity", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_BOUNDARY,  false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Boundary", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_VARCHAR,   false, true, true, FormatFormat.LenOneByte, DataFormat.OneByteLen), // "Character", format: length(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_LONGBINARY,false, true, false, FormatFormat.LenFourBytes, DataFormat.FourBytesLen), // "Binary", format: length(4), data: length(4), data(length)
                new TypeInfo(TdsType.TDS_LONGCHAR,  false, true, true, FormatFormat.LenFourBytes, DataFormat.FourBytesLen), // "Character", format: length(4), data: length(4), data(length)

                new TypeInfo(TdsType.TDS_DECN,      false, true, true, FormatFormat.Decimal, DataFormat.OneByteLen), // "Decimal", format: length(1), precision(1), scale(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_NUMN,      false, true, true, FormatFormat.Decimal, DataFormat.OneByteLen), // "Numeric", format: length(1), precision(1), scale(1), data: length(1), data(length)
                new TypeInfo(TdsType.TDS_IMAGE,     false, true, false, FormatFormat.Image, DataFormat.Image), // "Image", format: length(4), nameLength(2), name(nameLength), data: txtPtrLen(1), txtPtr, (timestamp(8), dataLen(4), data(dataLen)) (if not null)
                new TypeInfo(TdsType.TDS_TEXT,      false, true, true, FormatFormat.Image, DataFormat.Image), // "Text", format: length(4), nameLength(2), name(nameLength), data: txtPtrLen(1), txtPtr, (timestamp(8), dataLen(4), data(dataLen)) (if not null)
                //new TypeInfo(TdsType.TDS_BLOB,      false, true, false, FormatFormat.Blob, DataFormat.Blob), // "Serialized Object", format: blobType(1), classIdLen(2), classId(classIdLen), data: serializationType(1), classIdLen(2), classId(classIdLen), [dataLen(4), data(dataLen)](until dataLenThe high-order bit indicates whether this is the last (0))
                //new TypeInfo(TdsType.TDS_VOID,      false, false, false, "Void (unknown)"),
            };

            _types = types.ToDictionary(k => k.Type, v => v);
        }

        public static void ReadFormat(FormatToken.Format f, TdsResponseStream stream)
        {
            var t = _types[f.Type];

            if (t.FormatFormat == FormatFormat.LenOneByte || t.FormatFormat == FormatFormat.Decimal)
            {
                f.Len = stream.Read();
            }

            if (t.FormatFormat == FormatFormat.LenFourBytes || t.FormatFormat == FormatFormat.Image)
            {
                f.Len = stream.ReadInt();
            }

            if (t.FormatFormat == FormatFormat.Image)
            {
                int nameLen = stream.ReadShort();
                f.ObjectName = stream.ReadString(nameLen);
            }

            if (t.FormatFormat == FormatFormat.Decimal)
            {
                f.Pecision = stream.Read();
                f.Scale = stream.Read();
            }

            int localLen = stream.Read();
            f.LocaleInfo = stream.ReadString(localLen);

            // todo: blob
        }

        public static int GetFormatLen(TdsType type, string name, Encoding encoder)
        {
            var t = _types[type];
            int size = 8;
            if (name != null)
            {
                size += encoder.GetByteCount(name);
            }

            switch (t.FormatFormat)
            {
                case FormatFormat.Empty:
                    break;
                case FormatFormat.LenOneByte:
                    size += 1;
                    break;
                case FormatFormat.LenFourBytes:
                    size += 4;
                    break;
                case FormatFormat.Decimal:
                    size += 3;
                    break;
                case FormatFormat.Image:
                default:
                    throw new NotImplementedException($"Unsupported type {type}");
            }

            return size;
        }

        public static void WriteFormat(TdsParameter p, TdsRequestStream stream, Encoding encoder)
        {
            if (p.ParameterName != null)
            {
                var nameBytes = encoder.GetBytes(p.ParameterName);
                stream.Write((byte)nameBytes.Length);
                stream.Write(nameBytes);
            }
            else
            {
                stream.Write(0);
            }

            stream.Write((byte)(p.IsOutput ? 1 : 0));
            stream.WriteInt(0); // user type
            stream.Write((byte)p.TdsType);

            switch (p.TdsType)
            {
                case TdsType.TDS_VARBINARY:
                case TdsType.TDS_VARCHAR:
                    stream.Write((byte)VAR_MAX);
                    break;
                case TdsType.TDS_INTN:
                    if (p.DbType == System.Data.DbType.Int64)
                    {
                        stream.Write((byte)8);
                    }
                    else
                    {
                        stream.Write((byte)4);
                    }
                    break;
                case TdsType.TDS_FLTN:
                    if (p.DbType == System.Data.DbType.Single)
                    {
                        stream.Write((byte)4);
                    }
                    else
                    {
                        stream.Write((byte)8);
                    }
                    break;
                case TdsType.TDS_DATETIMEN:
                    stream.Write((byte)8);
                    break;
                case TdsType.TDS_DECN:
                    if (p.Value == null)
                    {
                        stream.Write((byte)17);
                        stream.Write((byte)38);
                        stream.Write((byte)0);
                    }
                    else
                    {
                        var sqlDec = new System.Data.SqlTypes.SqlDecimal((decimal)p.Value);
                        stream.Write((byte)17);
                        stream.Write(sqlDec.Precision);
                        stream.Write(sqlDec.Scale);
                    }
                    break;
                case TdsType.TDS_BIT:
                    break;
                case TdsType.TDS_MONEYN:
                default:
                    throw new NotImplementedException($"Unsupported type {p.TdsType}");
            }

            stream.Write((byte)0); // Locale information
        }

        public static void WriteData(TdsParameter p, TdsRequestStream stream, Encoding encoder)
        {
            switch (p.TdsType)
            {
                case TdsType.TDS_VARCHAR:
                    if (p.Value == null)
                    {
                        stream.Write((byte)0);
                    }
                    else
                    {
                        var s = (string)p.Value;
                        var bytes = encoder.GetBytes(s);
                        int strLen = Math.Max(VAR_MAX, bytes.Length);
                        stream.Write((byte)strLen);
                        stream.Write(bytes, 0, strLen);
                    }
                    break;
                case TdsType.TDS_VARBINARY:
                    if (p.Value == null)
                    {
                        stream.Write((byte)0);
                    }
                    else
                    {
                        var bytes = (byte[])p.Value;
                        int bytesLen = Math.Max(VAR_MAX, bytes.Length);
                        stream.Write((byte)bytesLen);
                        stream.Write(bytes, 0, bytesLen);
                    }
                    break;
                case TdsType.TDS_INTN:
                    if (p.Value == null)
                    {
                        stream.Write((byte)0);
                    }
                    else if (p.Value.GetType() == typeof(long))
                    {
                        stream.Write((byte)8);
                        stream.WriteLong((long)p.Value);
                    }
                    else // convert all smaller ints to int32
                    {
                        stream.Write((byte)4);
                        stream.WriteInt((int)p.Value);
                    }
                    break;
                case TdsType.TDS_FLTN:
                    if (p.Value == null)
                    {
                        stream.Write((byte)0);
                    }
                    else if (p.Value.GetType() == typeof(float))
                    {
                        stream.Write((byte)4);
                        stream.WriteInt(BitConverter.SingleToInt32Bits((float)p.Value));
                    }
                    else // double
                    {
                        stream.Write((byte)8);
                        stream.WriteLong(BitConverter.DoubleToInt64Bits((double)p.Value));
                    }
                    break;
                case TdsType.TDS_DATETIMEN:
                    if (p.Value == null)
                    {
                        stream.Write((byte)0);
                    }
                    else
                    {
                        stream.Write((byte)8);
                        var dt = new System.Data.SqlTypes.SqlDateTime((DateTime)p.Value);
                        stream.WriteInt(dt.DayTicks);
                        stream.WriteInt(dt.TimeTicks);
                    }
                    break;
                case TdsType.TDS_DECN:
                    if (p.Value == null)
                    {
                        stream.Write((byte)0);
                    }
                    else
                    {
                        var sqlDec = new System.Data.SqlTypes.SqlDecimal((decimal)p.Value);
                        stream.Write((byte)17);
                        stream.Write(sqlDec.IsPositive ? (byte)0 : (byte)1);
                        stream.Write(sqlDec.BinData.Reverse().ToArray());
                    }
                    break;
                case TdsType.TDS_MONEYN:
                    //if (value == null)
                    //{
                    stream.Write((byte)0);
                    //}
                    //else
                    //{
                    //}
                    break;
                case TdsType.TDS_BIT:
                    stream.Write((bool)p.Value ? (byte)1 : (byte)0);
                    break;
                default:
                    throw new NotImplementedException($"Unsupported type {p.TdsType}");
            }
        }

        public static object ReadData(FormatToken.Format f, TdsResponseStream stream, Encoding encoder)
        {
            int len;
            switch (f.Type)
            {
                case TdsType.TDS_INTN:
                case TdsType.TDS_UINTN: // lets ignore unsigned for now
                    switch (stream.Read())
                    {
                        case 1:
                            return stream.Read();
                        case 2:
                            return stream.ReadShort();
                        case 4:
                            return stream.ReadInt();
                        case 8:
                            return stream.ReadLong();
                    }
                    break;
                case TdsType.TDS_INT1:
                case TdsType.TDS_UINT1:
                    return stream.Read();
                case TdsType.TDS_INT2:
                case TdsType.TDS_UINT2:
                    return stream.ReadShort();
                case TdsType.TDS_INT4:
                case TdsType.TDS_UINT4:
                    return stream.ReadInt();
                case TdsType.TDS_INT8:
                case TdsType.TDS_UINT8:
                    return stream.ReadLong();
                case TdsType.TDS_BIT:
                    return stream.Read() != 0;
                case TdsType.TDS_DATETIMEN:
                case TdsType.TDS_DATETIME:
                case TdsType.TDS_SHORTDATE:
                    return GetDatetimeValue(stream, f.Type);
                case TdsType.TDS_FLT4:
                    return BitConverter.Int32BitsToSingle(stream.ReadInt());
                case TdsType.TDS_FLT8:
                    return BitConverter.Int64BitsToDouble(stream.ReadLong());
                case TdsType.TDS_FLTN:
                    len = stream.Read();

                    if (len == 4)
                    {
                        return BitConverter.Int32BitsToSingle(stream.ReadInt());
                    }
                    else if (len == 8)
                    {
                        return BitConverter.Int64BitsToDouble(stream.ReadLong());
                    }

                    break;
                case TdsType.TDS_SHORTMONEY:
                case TdsType.TDS_MONEY:
                case TdsType.TDS_MONEYN:
                    return GetMoneyValue(stream, f.Type);
                case TdsType.TDS_DECN:
                case TdsType.TDS_NUMN:
                    return GetDecimalValue(stream, f.Type, f);
                case TdsType.TDS_LONGBINARY:
                    len = stream.ReadInt();
                    return stream.ReadBytes(len);
                case TdsType.TDS_LONGCHAR:
                    len = stream.ReadInt();
                    if (len > 0)
                    {
                        return encoder.GetString(stream.ReadBytes(len));
                    }
                    break;
                case TdsType.TDS_IMAGE:
                    len = stream.Read();
                    if (len > 0)
                    {
                        var txtPtr = stream.ReadBytes(len);
                        var timeStamp = stream.ReadBytes(8);
                        int dataLen = stream.ReadInt();
                        return stream.ReadBytes(dataLen);
                    }
                    break;
                case TdsType.TDS_TEXT:
                    len = stream.Read();
                    if (len > 0)
                    {
                        var txtPtr = stream.ReadBytes(len);
                        var timeStamp = stream.ReadBytes(8);
                        int dataLen = stream.ReadInt();
                        return encoder.GetString(stream.ReadBytes(dataLen));
                    }
                    break;
                case TdsType.TDS_CHAR:
                case TdsType.TDS_VARCHAR:
                    len = stream.Read();
                    if (len > 0)
                    {
                        return encoder.GetString(stream.ReadBytes(len));
                    }
                    break;
                case TdsType.TDS_BINARY:
                case TdsType.TDS_VARBINARY:
                case TdsType.TDS_BOUNDARY:
                case TdsType.TDS_SENSITIVITY:
                    len = stream.Read();
                    return stream.ReadBytes(len);
                case TdsType.TDS_VOID:
                case TdsType.TDS_BLOB:
                default:
                    throw new NotImplementedException();
            }

            return null;
        }

        private static object GetDecimalValue(TdsResponseStream stream, TdsType type, FormatToken.Format f)
        {
            int len = stream.Read();

            if (len > 0)
            {
                var scale = Math.Pow(10, f.Scale ?? 0);

                bool sign = stream.Read() != 0;
                var bytes = stream.ReadBytes(len - 1);
                var bi = new BigInteger(bytes.Reverse().ToArray());

                var d = (decimal)bi;
                if (sign)
                {
                    d = -d;
                }

                return d / (decimal)scale;
            }
            else
            {
                return null;
            }
        }

        private static object GetMoneyValue(TdsResponseStream stream, TdsType type)
        {
            int len;

            if (type == TdsType.TDS_MONEY)
            {
                len = 8;
            }
            else if (type == TdsType.TDS_MONEYN)
            {
                len = stream.Read();
            }
            else
            {
                len = 4;
            }

            long? x = null;
            if (len == 4)
            {
                x = stream.ReadInt();
            }
            else if (len == 8)
            {
                int hi = stream.ReadInt();
                int lo = stream.ReadInt();
                x = lo | (hi << 32);
            }

            return x.HasValue ? new decimal(x.Value) * 0.0001m : (decimal?)null;
        }

        private static object GetDatetimeValue(TdsResponseStream stream, TdsType type)
        {
            int len;
            int dayTicks;
            int timeTicks;
            int minutes;

            if (type == TdsType.TDS_DATETIMEN)
            {
                len = stream.Read();
            }
            else if (type == TdsType.TDS_SHORTDATE)
            {
                len = 4;
            }
            else
            {
                len = 8;
            }

            switch (len)
            {
                case 8:
                    dayTicks = stream.ReadInt();
                    timeTicks = stream.ReadInt();
                    return new System.Data.SqlTypes.SqlDateTime(dayTicks, timeTicks).Value;
                case 4:
                    dayTicks = stream.ReadShort();
                    minutes = stream.ReadShort();
                    return new DateTime(1900, 1, 1).AddDays(dayTicks).AddMinutes(minutes);
                default:
                case 0:
                    return null;

            }
        }

        private class TypeInfo
        {
            public TypeInfo(TdsType type, bool fixedLen, bool nullable, bool converted, FormatFormat ff, DataFormat df)
            {
                Type = type;
                FixedLen = fixedLen;
                Nullable = nullable;
                Converted = converted;
                FormatFormat = ff;
                DataFormat = df;
            }

            public TdsType Type { get; }
            public bool FixedLen { get; }
            public bool Nullable { get; }
            public bool Converted { get; }
            public FormatFormat FormatFormat { get; }
            public DataFormat DataFormat { get; }
        }

        public enum FormatFormat
        {
            Empty,
            LenOneByte,
            LenFourBytes,
            Decimal,
            Image,
        }

        public enum DataFormat
        {
            OneByte,
            FourBytes,
            EightBytes,
            TwoBytes,
            OneByteLen,
            FourBytesLen,
            Image,
        }
    }
}
