using System;
using System.Data;

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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IMigrator<T> GetMigrator<T>() where T : class, new();
    }
}
