using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace TdsClient
{
    public class TdsCommand : IDbCommand
    {
        public TdsCommand()
        {
            Parameters = new TdsParameters();
        }

        public TdsCommand(string cmdText)
            : this()
        {
            CommandText = cmdText;
        }

        public TdsCommand(string cmdText, TdsConnection connection)
            : this(cmdText)
        {
            Connection = connection;
        }

        public TdsCommand(string cmdText, TdsConnection connection, TdsTransaction transaction)
            : this(cmdText, connection)
        {
            Transaction = transaction;
        }

        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; } = CommandType.Text;

        public TdsConnection Connection { get; set; }
        IDbConnection IDbCommand.Connection { get => Connection; set => Connection = (TdsConnection)value; }

        public TdsTransaction Transaction { get; set; }
        IDbTransaction IDbCommand.Transaction { get => Transaction; set => Transaction = (TdsTransaction)value; }

        public TdsParameters Parameters { get; private set; }
        IDataParameterCollection IDbCommand.Parameters => Parameters;

        public UpdateRowSource UpdatedRowSource { get; set; }

        public IDbDataParameter CreateParameter() => new TdsParameter();

        public void Prepare() => throw new NotImplementedException();

        public int ExecuteNonQuery()
        {
            Validate();
            using (var reader = Connection.Execute(CommandText, Parameters, CommandBehavior.Default, CommandTimeout))
            {
                return reader.RecordsAffected;
            }
        }

        public object ExecuteScalar()
        {
            Validate();
            using (var reader = Connection.Execute(CommandText, Parameters, CommandBehavior.SingleResult | CommandBehavior.SingleRow, CommandTimeout))
            {
                if (reader.Read())
                {
                    return reader[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            Validate();

            return Connection.Execute(CommandText, Parameters, behavior, CommandTimeout);
        }

        public void Cancel()
        {
            Connection.Cancel();
        }

        public void Dispose()
        {
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                throw new ArgumentException("No command text found");
            }

            if (Connection == null)
            {
                throw new ArgumentException("Connection is null");
            }

            if (CommandType != CommandType.Text)
            {
                throw new ArgumentException("Only text supported");
            }
        }

        public class TdsParameters : List<TdsParameter>, IDataParameterCollection
        {
            public object this[string parameterName]
            {
                get => this.FirstOrDefault(p => p.ParameterName == parameterName);
                set
                {
                    var existing = this.FirstOrDefault(p => p.ParameterName == parameterName);
                    if (existing != null)
                    {
                        Remove(existing);
                    }

                    Add((TdsParameter)value);
                }
            }

            public bool Contains(string parameterName) => this.Any(p => p.ParameterName == parameterName);

            public int IndexOf(string parameterName)
            {
                var existing = this.FirstOrDefault(p => p.ParameterName == parameterName);
                if (existing != null)
                {
                    return IndexOf(existing);
                }
                else
                {
                    return -1;
                }
            }

            public void RemoveAt(string parameterName)
            {
                var existing = this.FirstOrDefault(p => p.ParameterName == parameterName);
                if (existing != null)
                {
                    Remove(existing);
                }
            }
        }
    }
}
