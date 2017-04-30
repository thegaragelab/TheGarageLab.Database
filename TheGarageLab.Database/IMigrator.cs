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
    public interface IMigrator
    {
        /// <summary>
        /// Set up the migration.
        /// </summary>
        /// <param name="fromVersion"></param>
        /// <param name="model"></param>
        void BeginMigration(int fromVersion, Type model);

        /// <summary>
        /// Migrate a single record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        object MigrateRecord(IDataRecord record);
    }
}
