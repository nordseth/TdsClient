using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TdsClient
{
    public class TdsTransaction : IDbTransaction
    {
        public TdsTransaction(TdsConnection connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            IsolationLevel = isolationLevel;
            Connection = connection;
            Begin();
        }

        public TdsConnection Connection { get; private set; }
        IDbConnection IDbTransaction.Connection => Connection;
        public IsolationLevel IsolationLevel { get; private set; }

        public void Commit()
        {
            Connection.ExecuteNonQuery("COMMIT TRANSACTION;", null);
            Connection = null;
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Commit();
            }
        }

        public void Rollback()
        {
            Connection.ExecuteNonQuery("ROLLBACK TRANSACTION;", null);
            Connection = null;
        }

        private void Begin()
        {
            Connection.ExecuteNonQuery("BEGIN TRANSACTION;", null);
        }
    }
}
