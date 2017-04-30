using System.Data;
using TheGarageLab.Logging;

namespace TheGarageLab.Database.SqlLogger
{
    internal class DbCommandLogger : IDbCommand
    {
        /// <summary>
        /// The logger to use for output
        /// </summary>
        private readonly ILogger Logger;

        /// <summary>
        /// The logging severity to use
        /// </summary>
        private readonly Severity Severity;

        /// <summary>
        /// The inner command
        /// </summary>
        private readonly IDbCommand Inner;

        /// <summary>
        /// Constructor with logging information and command instance to wrap
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        /// <param name="severity"></param>
        public DbCommandLogger(ILogger logger, IDbCommand command, Severity severity)
        {
            Logger = logger;
            Inner = command;
            Severity = severity;
        }

        #region Implementation of IDbCommand
        public string CommandText
        {
            get
            {
                return Inner.CommandText;
            }

            set
            {
                Inner.CommandText = value;
            }
        }

        public int CommandTimeout
        {
            get
            {
                return Inner.CommandTimeout;
            }

            set
            {
                Inner.CommandTimeout = value;
            }
        }

        public CommandType CommandType
        {
            get
            {
                return Inner.CommandType;
            }

            set
            {
                Inner.CommandType = value;
            }
        }

        public IDbConnection Connection
        {
            get
            {
                return Inner.Connection;
            }

            set
            {
                Inner.Connection = value;
            }
        }

        public IDataParameterCollection Parameters
        {
            get
            {
                return Inner.Parameters;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return Inner.Transaction;
            }

            set
            {
                Inner.Transaction = value;
            }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get
            {
                return Inner.UpdatedRowSource;
            }

            set
            {
                Inner.UpdatedRowSource = value;
            }
        }

        public void Cancel()
        {
            Inner.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return Inner.CreateParameter();
        }

        public void Dispose()
        {
            Inner.Dispose();
        }

        public int ExecuteNonQuery()
        {
            return Inner.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            return Inner.ExecuteReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return Inner.ExecuteReader(behavior);
        }

        public object ExecuteScalar()
        {
            return Inner.ExecuteScalar();
        }

        public void Prepare()
        {
            Inner.Prepare();
        }
        #endregion
    }
}
