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
        /// Determine if migration is required for the given type.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool MigrationRequired(IDbConnection db, Type t)
        {
            // Get the current version of the model
            int currentVersion = 1;
            try
            {
                currentVersion = (int)t.GetField("VERSION", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null);
            }
            catch (Exception)
            {
                // Ignore exceptions and assume version 1
            }
            // Get the version according to the schema record
            List<SchemaMetadata> tableInfo = db.Select<SchemaMetadata>(x => (x.Table == t.GetModelMetadata().ModelName) && (x.Version != currentVersion));
            if (tableInfo.Count == 0)
                return false;
            if (tableInfo[0].Version > currentVersion)
                throw new InvalidOperationException("Table cannot be migrated to an older version.");
            return true;
        }

        /// <summary>
        /// Migrate a single table.
        /// </summary>
        /// <param name="t"></param>
        private void MigrateSingleTable(Type t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create any new tables and return a list of those that
        /// require migration.
        /// </summary>
        /// <param name="models"></param>
        private List<Type> CreateNewTables(Type[] models)
        {
            var toMigrate = new List<Type>();
            using (var db = Open())
            {
                // Create any new tables and determine which ones require migration
                foreach (var t in models)
                {
                    if (!MigrationRequired(db, t))
                        db.CreateTableIfNotExists(t);
                    else
                        toMigrate.Add(t);
                }
            }
            return toMigrate;
        }

        /// <summary>
        /// Revert a migration, restoring the original data.
        /// </summary>
        /// <param name="t"></param>
        private void RevertSingleMigration(Type t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Revert a set of migrations (in reverse order). Returns
        /// a list of exceptions that occurred during the process.
        /// </summary>
        /// <param name="migrated"></param>
        /// <returns></returns>
        private List<Exception> RevertMigrations(List<Type> migrated)
        {
            var exceptions = new List<Exception>();
            // Revert the migrations
            migrated.Reverse();
            foreach (var t in migrated)
            {
                try
                {
                    RevertSingleMigration(t);
                }
                catch (Exception reversionException)
                {
                    exceptions.Add(reversionException);
                }
            }
            // All done
            return exceptions;
        }

        /// <summary>
        /// Migrate the tables in the list
        /// </summary>
        /// <param name="models"></param>
        private void MigrateTables(List<Type> toMigrate)
        {
            // Migrate any tables that require it
            var migrated = new List<Type>();
            try
            {
                foreach (var t in toMigrate)
                {
                    MigrateSingleTable(t);
                    migrated.Add(t);
                }
            }
            catch (Exception primaryException)
            {
                // Try and revert the migrations already done
                var exceptions = RevertMigrations(migrated);
                if (exceptions.Count > 0)
                {
                    // Errors occured during reversion, throw an aggregate
                    exceptions.Insert(0, primaryException);
                    throw new AggregateException(exceptions);
                }
                // Just throw the original exception
                throw primaryException;
            }
        }

        /// <summary>
        /// Update the schema metadata with the current models
        /// </summary>
        /// <param name="models"></param>
        private void UpdateSchema(Type[] models)
        {
            throw new NotImplementedException();
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
            // Make sure we have the schema table
            using (var db = Open())
            {
                db.CreateTableIfNotExists<SchemaMetadata>();
            }
            // Create any new tables and list those that require migration
            var toMigrate = CreateNewTables(models);
            // Migrate tables
            MigrateTables(toMigrate);
            // Finally, update the schema
            try
            {
                UpdateSchema(models);
            }
            catch (Exception ex)
            {
                // Rollback migrations
                var exceptions = RevertMigrations(new List<Type>(models));
                if (exceptions.Count > 0)
                {
                    // Exceptions occured during rollback, throw an aggregate
                    exceptions.Insert(0, ex);
                    throw new AggregateException(exceptions);
                }
                // Just throw the original exception
                throw ex;
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
