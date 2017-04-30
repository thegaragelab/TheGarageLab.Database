using System;
using System.IO;
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
        /// Connection string for in-memory databases
        /// </summary>
        public const string MEMORY_DATABASE = ":memory:";

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
        /// Create a backup of the database and return the name of the
        /// backup file.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        private string BackupDatabase(string database)
        {
            // Special case for in memory database or no database yet
            if ((database == MEMORY_DATABASE) || !File.Exists(database))
                return null;
            // Create a backup of the file
            string backupFile = Path.Combine(
                Path.GetDirectoryName(database),
                string.Format("{0}-{1}{2}",
                    Path.GetFileNameWithoutExtension(database),
                    DateTime.UtcNow.ToString("YYYYMMddHHmmss"),
                    Path.GetExtension(database)
                    )
                );
            File.Copy(database, backupFile);
            return backupFile;
        }

        /// <summary>
        /// Restore from a backup database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="backupFile"></param>
        private void RestoreBackup(string database, string backupFile)
        {
            // Don't restore if no backup created
            if ((backupFile == null) || !File.Exists(backupFile))
                return;
            // Remove the original
            if (File.Exists(database))
                File.Delete(database);
            File.Move(backupFile, database);
        }

        /// <summary>
        /// Remove the backup file if it exists
        /// </summary>
        /// <param name="backupFile"></param>
        private void RemoveBackup(string backupFile)
        {
            // Don't restore if no backup created
            if ((backupFile == null) || !File.Exists(backupFile))
                return;
            File.Delete(backupFile);
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
            // We have changes to make so back up the existing database
            string backupDatabase = BackupDatabase(connectionString);
            try
            {
                MigrateTables(metadata, migrations);
                CreateNewTables(metadata, creations);
            }
            catch (Exception ex)
            {
                // Restore from backup
                Logger.Error("Database creation/migration failed. Restoring backup.");
                try
                {
                    RestoreBackup(connectionString, backupDatabase);
                }
                catch (Exception ex2)
                {
                    Logger.Fatal(ex2, "Could not restore from backup database.");
                }
                finally
                {
                    throw ex;
                }
            }
            // Remove the backup file
            RemoveBackup(backupDatabase);
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
