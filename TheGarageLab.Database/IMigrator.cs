using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGarageLab.Database
{
    /// <summary>
    /// Provides a mechanism for migrating a table from one
    /// version to another.
    /// </summary>
    public interface IMigrator<T> where T : class, new()
    {
        /// <summary>
        /// Migrate a single record.
        /// </summary>
        /// <param name="fromVersion"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        T MigrateRecord(int fromVersion, IDataRecord record);
    }
}
