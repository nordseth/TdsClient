using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class FormatToken : TdsToken
    {
        public FormatToken(TokenType tokenType) : base(tokenType)
        {
        }

        public List<Format> Formats { get; internal set; }

        public class Format
        {
            public bool Params { get; internal set; }

            public string Name { get; internal set; }
            public string CatalogName { get; internal set; }
            public string SchemaName { get; internal set; }
            public string TableName { get; internal set; }
            public string ColumnName { get; internal set; }

            public UserDataType UserType { get; internal set; }
            public TdsType Type { get; internal set; }

            // TDS_PARAM_RETURN 0x01 This is a return parameter.It is like a parameter passed by reference.
            // TDS_PARAM_NULLALLOWED 0x20 This parameter can be NULL

            // TDS_ROW_HIDDEN 0x01 This is a hidden column.It was not listed in the target list of the select statement.
            //      Hidden fields are often used to pass key information back to a client.For example: select a, b from
            //      table T where columns b and c are the key columns.Columns a, b, and c may be returned and c would have a status of
            // TDS_ROW_HIDDEN|TDS_ROW_KEY.
            // TDS_ROW_KEY 0x02 This indicates that this column is a key.
            // TDS_ROW_VERSION 0x04 This column is part of the version key for a row. It is used when updating rows through cursors.
            // TDS_ROW_UPDATABLE 0x10 This column is updatable.It is used with cursors.
            // TDS_ROW_NULLALLOWED 0x20 This column allows nulls.
            // TDS_ROW_IDENTITY 0x40 This column is an identity column.
            // TDS_ROW_PADCHAR 0x80 This column has
            public int Status { get; internal set; }

            public int? Len { get; internal set; }
            public int? Pecision { get; internal set; }
            public int? Scale { get; internal set; }
            public string LocaleInfo { get; internal set; }
            public string ObjectName { get; internal set; }
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append(TokenType);
            s.Append(": [");

            foreach (var f in Formats)
            {
                s.Append($" {{ {f.Type}/{f.UserType}({f.Len}{(f.Pecision.HasValue ? $", {f.Pecision}" : null)}{(f.Scale.HasValue ? $", {f.Scale}" : null)}) \"{f.Name}\" {f.LocaleInfo}{f.ObjectName} }}");
            }

            s.Append(" ]");
            return s.ToString();
        }

        public override string ToShortString()
        {
            return $"{TokenType}({Formats.Count})";
        }
    }
}
