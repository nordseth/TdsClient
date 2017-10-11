using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient
{
    public class TdsParameter : DbParameter
    {
        private string _name;
        private DbType? _dbType;
        private TdsType? _tdsType;
        private object _value;

        public TdsParameter()
        {
            Direction = ParameterDirection.Input;
        }

        public TdsParameter(string name, DbType type, object value)
        {
            Direction = ParameterDirection.Input;
            ParameterName = name;
            DbType = type;
            Value = value;
        }

        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != null && !value.StartsWith("@"))
                {
                    _name = "@" + value;
                }
                else
                {
                    _name = value;
                }
            }
        }
        public override int Size { get; set; }
        public override string SourceColumn { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public bool IsOutput => Direction == ParameterDirection.InputOutput || Direction == ParameterDirection.Output;

        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value != null && value.GetType() == typeof(char))
                {
                    value = value.ToString();
                }
                else if (value != null && value.GetType() == typeof(char[]))
                {
                    value = new string((char[])value);
                }

                _value = value;
                if (!_dbType.HasValue)
                {
                    DbType = GetDbType(_value);
                }
            }
        }

        public override DbType DbType
        {
            get
            {
                return _dbType ?? DbType.AnsiString;
            }
            set
            {
                _dbType = value;
                if (!_tdsType.HasValue)
                {
                    _tdsType = GetTdsType();
                }
            }
        }
        public TdsType TdsType
        {
            get
            {
                return _tdsType ?? TdsType.TDS_VARCHAR;
            }
            set
            {
                _tdsType = value;
            }
        }

        public override void ResetDbType()
        {
            _dbType = null;
            _tdsType = null;
        }

        private DbType GetDbType(object value)
        {
            if (value == null)
            {
                return DbType.AnsiString;
            }

            if (value.GetType() == typeof(byte[]))
            {
                return DbType.Binary;
            }

            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Char:
                    return DbType.StringFixedLength;
                case TypeCode.String:
                    return DbType.String;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                default:
                    throw new NotImplementedException($"Unsupported type {value.GetType()}");
            }
        }

        private TdsType GetTdsType()
        {
            switch (DbType)
            {
                case DbType.Binary:
                    return TdsType.TDS_VARBINARY;
                case DbType.Boolean:
                    return TdsType.TDS_BIT;
                case DbType.Currency:
                    return TdsType.TDS_MONEYN;
                case DbType.DateTime:
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTime2:
                    return TdsType.TDS_DATETIMEN;
                case DbType.Decimal:
                    return TdsType.TDS_DECN;
                case DbType.Double:
                case DbType.Single:
                    return TdsType.TDS_FLTN;
                case DbType.Byte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                    return TdsType.TDS_INTN;
                case DbType.AnsiString:
                case DbType.String:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return TdsType.TDS_VARCHAR;
                default:
                    throw new NotImplementedException($"Unsupported (db) type {DbType}");
            }
        }
    }
}
