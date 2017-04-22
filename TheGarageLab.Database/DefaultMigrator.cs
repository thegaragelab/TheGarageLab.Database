using System;
using System.Data;

namespace TheGarageLab.Database
{
    /// <summary>
    /// Generic migrator. This implementation uses a brute
    /// force approach to migrate the table (which is generally
    /// good enough for simple changes).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultMigrator<T> : IMigrator<T> where T : class, new()
    {
        /// <summary>
        /// Migrate a single record.
        /// </summary>
        /// <param name="fromVersion"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public T MigrateRecord(int fromVersion, IDataRecord record)
        {
            throw new NotImplementedException();
        }
    }
}
