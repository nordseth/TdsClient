using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TdsClient
{
    public class TdsConnection : IDbConnection
    {
        private const string InitConnectionSql = "SET TRANSACTION ISOLATION LEVEL 1\r\n" +
            "SET CHAINED OFF\r\n" +
            "SET QUOTED_IDENTIFIER ON\r\n" +
            "SET TEXTSIZE 2147483647";

        private string _connectionString;
        private Protocol.RawConnection _raw;
        private ILoggerFactory _loggerFactory;
        private ILogger<TdsConnection> _logger;

        public TdsConnection() : this(null, null)
        {
        }

        public TdsConnection(string connectionString) : this(connectionString, null)
        {
        }

        public TdsConnection(string connectionString, ILoggerFactory loggerFactory)
        {
            ConnectionString = connectionString;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger<TdsConnection>();
        }

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (State == ConnectionState.Closed)
                {
                    _connectionString = value;
                }
                else
                {
                    throw new ArgumentException("Can only set connection string on a colosed connection");
                }
            }
        }

        public int ConnectionTimeout { get; private set; } = 60;
        public string Database { get; private set; }
        public ConnectionState State { get; private set; } = ConnectionState.Closed;
        public ConnectionParamaters ConnectionParamaters { get; private set; }

        public IDbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

        public void Open()
        {
            if (State != ConnectionState.Closed)
            {
                throw new ArgumentException("Can only open a colosed connection");
            }

            State = ConnectionState.Connecting;
            ConnectionParamaters = new ConnectionParamaters(ConnectionString);

            _logger?.LogDebug($"Connecting...");
            _raw = new Protocol.RawConnection(ConnectionParamaters, _loggerFactory);
            _raw.Connect();
            _logger?.LogDebug($"Connected");
            _raw.SendLogin();
            _logger?.LogDebug($"Log in sent... reading response");
            _raw.ReadLoginResponse();
            _logger?.LogInformation($"Logged in, connection open");

            State = ConnectionState.Open;

            ExecuteNonQuery(InitConnectionSql, null);

            if (!string.IsNullOrEmpty(ConnectionParamaters.DatabaseName))
            {
                ChangeDatabase(ConnectionParamaters.DatabaseName);
            }
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return new TdsTransaction(this, il);
        }

        public void ChangeDatabase(string databaseName)
        {
            ExecuteNonQuery($"USE {databaseName}", null);
        }

        public IDbCommand CreateCommand()
        {
            return new TdsCommand(null, this);
        }

        public void Cancel()
        {
            if (State == ConnectionState.Executing || State == ConnectionState.Fetching)
            {
                _raw.SendCancel();
                _raw.WaitForCancel();
            }
        }

        public void Close()
        {
            if (State == ConnectionState.Closed)
            {
                return;
            }

            _logger?.LogDebug($"Disconnecting...");
            if (State == ConnectionState.Executing || State == ConnectionState.Fetching)
            {
                _raw.SendCancel();
            }

            _raw.Dispose();
            _raw = null;
            State = ConnectionState.Closed;
        }

        public void Dispose()
        {
            Close();
        }

        public void ExecuteNonQuery(string sql, IEnumerable<TdsParameter> parameters)
        {
            using (var c = new TdsCommand(sql, this))
            {
                if (parameters != null)
                {
                    c.Parameters.AddRange(parameters);
                }

                c.ExecuteNonQuery();
            }
        }

        public IDataReader Execute(string sql, IEnumerable<TdsParameter> parameters, CommandBehavior behavior, int timeout)
        {
            if (State != ConnectionState.Open)
            {
                throw new ArgumentException("Connection must be open");
            }

            State = ConnectionState.Executing;
            _raw.ExecuteSql(sql, parameters);
            State = ConnectionState.Fetching;
            var tokens = _raw.GetResponse();

            bool isError = _raw.ResponseIsError;
            var messages = _raw.ResponseMessages;
            var encoder = _raw.Encoder;

            State = ConnectionState.Open;
            if ((behavior & CommandBehavior.CloseConnection) != 0)
            {
                Close();
            }

            if (isError)
            {
                var errorMsg = messages.LastOrDefault(m => m.State > 10);
                throw new TdsDbException(errorMsg);
            }

            UpdateOutputParams(tokens, parameters);

            return new TdsDataReader(tokens, encoder);
        }

        private void UpdateOutputParams(IEnumerable<Protocol.TdsToken> tokens, IEnumerable<TdsParameter> parameters)
        {
            // todo
        }
    }
}
