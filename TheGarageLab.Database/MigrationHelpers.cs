using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace TheGarageLab.Database
{
    /// <summary>
    /// Helper methods for low level manipulation of the database
    /// </summary>
    internal static class MigrationHelpers
    {
        /// <summary>
        /// Drop a table from the database
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="name"></param>
        public static void DropTable(IDbConnection connection, string name)
        {
            connection.ExecuteSql($"DROP TABLE '{name}'");
        }

        /// <summary>
        /// Rename a table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public static void RenameTable(IDbConnection connection, string oldName, string newName)
        {
            connection.ExecuteSql($"ALTER TABLE '{oldName}' RENAME TO '{newName}'");
        }
    }
}
