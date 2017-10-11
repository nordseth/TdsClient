using System;
using System.Collections.Generic;
using System.Text;
using TdsClient.Protocol;

namespace TdsClient.Tokens
{
    public class DoneToken : TdsToken
    {
        public DoneToken(TokenType tokenType) : base(tokenType)
        {
        }

        public int? RowCount { get; internal set; }
        public bool More { get; internal set; }
        public bool Error { get; internal set; }
        public bool InTransaction { get; internal set; }
        public bool Proc { get; internal set; }
        public bool Count { get; internal set; }
        public bool AttentionAck { get; internal set; }
        public TransactionStatus TransactionState { get; internal set; }

        public override bool IsEndOfResponse()
        {
            return !More;
        }

        public enum TransactionStatus
        {
            TDS_NOT_IN_TRAN = 0, // Not currently in a transaction
            TDS_TRAN_SUCCEED = 1, // Request caused transaction to complete successfully.
            TDS_TRAN_PROGRESS = 2, // A transaction is still in progress on this dialog.
            TDS_STMT_ABORT = 3, // Request caused a statement abort to occur.
            TDS_TRAN_ABORT = 4, // Request caused transaction to abort.
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append(TokenType);
            s.Append(": {");

            if (More)
            {
                s.Append(" More");
            }

            if (Error)
            {
                s.Append(" Error");
            }

            if (Count)
            {
                s.AppendFormat(" rowCount: {0}", RowCount);
            }

            s.Append(" }");
            return s.ToString();
        }

        public override string ToShortString()
        {
            return ToString();
        }
    }
}
