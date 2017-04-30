using System;
using System.Data;
using System.Collections.Generic;

namespace TheGarageLab.Database
{
    public interface IDatabase
    {
        /// <summary>
        /// Create (or upgrade) the database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="models"></param>
        void Create(string connectionString, params Type[] models);

        /// <summary>
        /// Open a connection to the database
        /// </summary>
        /// <returns></returns>
        IDbConnection Open();

        /// <summary>
        /// Get a migrator for a given type.
        /// </summary>
        /// <returns></returns>
        IMigrator GetMigrator(Type t);

        /// <summary>
        /// Get information about a table by it's name
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        TableInfo GetTableInfo(string table);

        /// <summary>
        /// Get information about a table from the model type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        TableInfo GetTableInfo(Type model);

        /// <summary>
        /// Get information about all tables
        /// </summary>
        /// <returns></returns>
        List<TableInfo> GetTables();
    }
}
