using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TheGarageLab.Logging;

namespace TheGarageLab.Database.SqlLogger
{
    [ExcludeFromCodeCoverage]
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
            var watch = Stopwatch.StartNew();
            var result = Inner.ExecuteNonQuery();
            watch.Stop();
            Logger.Write(Severity, $"SQL ({watch.ElapsedMilliseconds}ms): {CommandText}");
            return result;
        }

        public IDataReader ExecuteReader()
        {
            var watch = Stopwatch.StartNew();
            var result = Inner.ExecuteReader();
            watch.Stop();
            Logger.Write(Severity, $"SQL ({watch.ElapsedMilliseconds}ms): {CommandText}");
            return result;
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var watch = Stopwatch.StartNew();
            var result = Inner.ExecuteReader(behavior);
            watch.Stop();
            Logger.Write(Severity, $"SQL ({watch.ElapsedMilliseconds}ms): {CommandText}");
            return result;
        }

        public object ExecuteScalar()
        {
            var watch = Stopwatch.StartNew();
            var result = Inner.ExecuteScalar();
            watch.Stop();
            Logger.Write(Severity, $"SQL ({watch.ElapsedMilliseconds}ms): {CommandText}");
            return result;
        }

        public void Prepare()
        {
            Inner.Prepare();
        }
        #endregion
    }
}
