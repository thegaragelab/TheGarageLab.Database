using System;
using System.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using TheGarageLab.Ensures;
using TheGarageLab.Logging;

namespace TheGarageLab.Database
{
    public class Database : IDatabase
    {
        /// <summary>
        /// Logging implementation
        /// </summary>
        private readonly ILogger Logger;

        /// <summary>
        /// The connection factory to use to create database connections
        /// </summary>
        private OrmLiteConnectionFactory ConnectionFactory;

        /// <summary>
        /// Constructor with injectable dependencies
        /// </summary>
        /// <param name="logger"></param>
        public Database(ILogger logger)
        {
            Logger = logger;
        }

        #region Helpers
        /// <summary>
        /// Migrate a single table.
        /// </summary>
        /// <param name="t"></param>
        private void MigrateSingleTable(Type t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Migrate the tables in the list
        /// </summary>
        /// <param name="models"></param>
        private void MigrateTables(SchemaManager manager, List<Type> models)
        {
            foreach (var model in models)
            {
                MigrateSingleTable(model);
                manager.UpdateMetadata(model);
            }
        }

        /// <summary>
        /// Create any new tables and return a list of those that
        /// require migration.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="models"></param>
        private void CreateNewTables(SchemaManager manager, List<Type> models)
        {
            foreach (var model in models)
            {
                using (var conn = Open())
                {
                    conn.CreateTableIfNotExists(model);
                }
                manager.UpdateMetadata(model);
            }
        }
        #endregion

        #region Implementation of IDatabase
        /// <summary>
        /// Create (or upgrade) the database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="models"></param>
        public void Create(string connectionString, params Type[] models)
        {
            Ensure.IsNull<InvalidOperationException>(ConnectionFactory);
            ConnectionFactory = new OrmLiteConnectionFactory(
                connectionString,
                SqliteDialect.Provider
                );
            // Determine what changes need to be made
            List<Type> creations;
            List<Type> migrations;
            var metadata = new SchemaManager(Logger, this);
            if (!metadata.GetRequiredChanges(models, out creations, out migrations))
                return; // Nothing to do
            // TODO: We have changes to make so back up the existing database
            try
            {
                MigrateTables(metadata, migrations);
                CreateNewTables(metadata, creations);
            }
            catch (Exception)
            {
                // TODO: Restore from backup
                throw;
            }
        }

        /// <summary>
        /// Open a connection to the database
        /// </summary>
        /// <returns></returns>
        public IDbConnection Open()
        {
            Ensure.IsNotNull<InvalidOperationException>(ConnectionFactory);
            return ConnectionFactory.Open();
        }

        /// <summary>
        /// Get a migrator for a given type. This implementation allows for
        /// models that implement their own migration method and falls back
        /// to a default migration tactic. Child classes may override this
        /// to determine the appropriate migrators in a different way.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IMigrator<T> GetMigrator<T>() where T : class, new()
        {
            // See if the type provides it's own migrator
            var t = typeof(T);
            if (typeof(IMigrator<T>).IsAssignableFrom(t))
                return new T() as IMigrator<T>;
            // Use the generic one
            return new DefaultMigrator<T>();
        }
        #endregion
    }
}
