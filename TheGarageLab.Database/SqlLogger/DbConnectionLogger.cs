using System.Data;
using TheGarageLab.Logging;

namespace TheGarageLab.Database.SqlLogger
{
    class DbConnectionLogger : IDbConnection
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
        private readonly IDbConnection Inner;

        /// <summary>
        /// Constructor with logging information and command instance to wrap
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connection"></param>
        /// <param name="severity"></param>
        public DbConnectionLogger(ILogger logger, IDbConnection connection, Severity severity)
        {
            Logger = logger;
            Inner = connection;
            Severity = severity;
        }

        #region Implementation of IDbConnection
        public string ConnectionString
        {
            get
            {
                return Inner.ConnectionString;
            }

            set
            {
                Inner.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return Inner.ConnectionTimeout;
            }
        }

        public string Database
        {
            get
            {
                return Inner.Database;
            }
        }

        public ConnectionState State
        {
            get
            {
                return Inner.State;
            }
        }

        public IDbTransaction BeginTransaction()
        {
            return Inner.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return Inner.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            Inner.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            Inner.Close();
        }

        public IDbCommand CreateCommand()
        {
            return new DbCommandLogger(
                Logger,
                Inner.CreateCommand(),
                Severity
                );
        }

        public void Dispose()
        {
            Inner.Dispose();
        }

        public void Open()
        {
            Inner.Open();
        }
        #endregion
    }
}
