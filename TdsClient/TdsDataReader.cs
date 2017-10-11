using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TdsClient.Protocol;
using TdsClient.Tokens;

namespace TdsClient
{
    public class TdsDataReader : IDataReader
    {
        private Encoding _encoder;
        private List<Tuple<FormatToken, List<DataToken>, DoneToken>> _resultSets;
        private int _resultSetIndex = -1;
        private Tuple<FormatToken, List<DataToken>, DoneToken> _currentResultSet;
        private int _rowIndex = -1;
        private DataToken _currentRow;

        public TdsDataReader(IEnumerable<TdsToken> tokens, Encoding encoder)
        {
            _encoder = encoder;
            _resultSets = InitResultSets(tokens);
            NextResult();
        }

        private List<Tuple<FormatToken, List<DataToken>, DoneToken>> InitResultSets(IEnumerable<TdsToken> tokens)
        {
            var r = new List<Tuple<FormatToken, List<DataToken>, DoneToken>>();

            FormatToken fmt = null;
            List<DataToken> rows = null;
            foreach (var t in tokens)
            {
                if (t is FormatToken)
                {
                    var newFormat = (FormatToken)t;
                    if (t.TokenType == TokenType.TDS_ROWFMT || t.TokenType == TokenType.TDS_ROWFMT2)
                    {
                        fmt = newFormat;
                        rows = new List<DataToken>();
                    }
                }
                else if (t is DataToken && fmt != null)
                {
                    var dataToken = (DataToken)t;
                    if (dataToken.TokenType == TokenType.TDS_ROW)
                    {
                        rows.Add(dataToken);
                    }
                }
                else if (t is DoneToken)
                {
                    if (fmt != null)
                    {
                        r.Add(new Tuple<FormatToken, List<DataToken>, DoneToken>(fmt, rows, (DoneToken)t));
                        fmt = null;
                        rows = null;
                    }
                }
            }

            return r;
        }

        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));

        public int Depth => 0;

        public bool IsClosed => _currentResultSet == null;

        public int RecordsAffected => _currentResultSet?.Item3.RowCount ?? 0;

        public int FieldCount => _currentResultSet?.Item1.Formats.Count ?? -1;

        public void Close()
        {
            _currentResultSet = null;
            _resultSets = null;
        }

        public void Dispose() => Close();

        public bool NextResult()
        {
            _resultSetIndex++;

            if (_resultSets != null && _resultSets.Count > _resultSetIndex)
            {
                _currentResultSet = _resultSets[_resultSetIndex];
                _rowIndex = -1;
                _currentRow = null;
                return true;
            }
            else
            {
                Close();
                return false;
            }
        }

        public bool Read()
        {
            if (_currentResultSet == null)
            {
                return false;
            }
            _rowIndex++;

            if (_currentResultSet.Item2.Count > _rowIndex)
            {
                _currentRow = _currentResultSet.Item2[_rowIndex];
                return true;
            }
            else
            {
                Close();
                return false;
            }
        }

        public string GetName(int i)
        {
            if (_currentResultSet == null)
            {
                throw new ArgumentException("Not in a result set");
            }
            else
            {
                return _currentResultSet.Item1.Formats[i].Name;
            }
        }

        public int GetOrdinal(string name)
        {
            if (_currentResultSet == null)
            {
                throw new ArgumentException("Not in a result set");
            }
            else
            {
                return _currentResultSet.Item1.Formats.IndexOf(_currentResultSet.Item1.Formats.FirstOrDefault(f => f.Name == name));
            }
        }

        public DataTable GetSchemaTable() => null;

        public IDataReader GetData(int i) => null;

        public string GetDataTypeName(int i)
        {
            if (_currentResultSet == null)
            {
                throw new ArgumentException("Not in a result set");
            }
            else
            {
                return _currentResultSet.Item1.Formats[i].UserType.ToString();
            }
        }

        public Type GetFieldType(int i)
        {
            return typeof(object);
        }

        public object GetValue(int i)
        {
            if (_currentResultSet == null)
            {
                throw new ArgumentException("Not in a result set");
            }
            else if (_currentRow == null)
            {
                throw new ArgumentException("Not in a row");
            }
            else
            {
                return _currentRow.Data[i];
            }
        }

        public int GetValues(object[] values) => throw new NotImplementedException();

        public bool GetBoolean(int i) => throw new NotImplementedException();

        public byte GetByte(int i) => throw new NotImplementedException();

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();

        public char GetChar(int i) => throw new NotImplementedException();

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotImplementedException();

        public DateTime GetDateTime(int i) => throw new NotImplementedException();

        public decimal GetDecimal(int i) => throw new NotImplementedException();

        public double GetDouble(int i) => throw new NotImplementedException();

        public float GetFloat(int i) => throw new NotImplementedException();

        public Guid GetGuid(int i) => throw new NotImplementedException();

        public short GetInt16(int i) => throw new NotImplementedException();

        public int GetInt32(int i) => throw new NotImplementedException();

        public long GetInt64(int i) => throw new NotImplementedException();

        public string GetString(int i) => throw new NotImplementedException();

        public bool IsDBNull(int i) => throw new NotImplementedException();
    }
}
