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
        /// Determine the name of the backup file to use. Will
        /// return null if there is nothing to backup.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        private string GetBackupFilename(string database)
        {
            // Must have an existing file database to backup
            if ((database == MEMORY_DATABASE) || !File.Exists(database))
                return null;
            return Path.Combine(
                Path.GetDirectoryName(database),
                string.Format("{0}-{1}{2}",
                    Path.GetFileNameWithoutExtension(database),
                    DateTime.UtcNow.ToString("YYYYMMddHHmmss"),
                    Path.GetExtension(database)
                    )
                );
        }

        /// <summary>
        /// Create a backup of the database and return the name of the
        /// backup file.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="backupFile"></param>
        /// <returns></returns>
        private void BackupDatabase(string database, string backupFile)
        {
            // Create a backup of the file
            if (backupFile != null)
                File.Copy(database, backupFile);
        }

        /// <summary>
        /// Restore from a backup database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="backupFile"></param>
        private void RestoreBackup(string database, string backupFile)
        {
            // Remove the original database
            if (File.Exists(database))
                File.Delete(database);
            // Restore if we have a back up
            if ((backupFile != null) && File.Exists(backupFile))
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

        /// <summary>
        /// Remove redundant tables
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="tables"></param>
        private void RemoveTables(SchemaManager manager, List<string> tables)
        {
            foreach (var table in tables)
            {
                using (var conn = Open())
                    MigrationHelpers.DropTable(conn, table);
                manager.RemoveMetadata(table);
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
            // Create a backup in case we need to make changes
            string backupDatabase = GetBackupFilename(connectionString);
            // Set up the database
            ConnectionFactory = new OrmLiteConnectionFactory(
                connectionString,
                SqliteDialect.Provider
                );
            // Determine what changes need to be made
            List<Type> creations;
            List<Type> migrations;
            List<string> removals;
            var metadata = new SchemaManager(Logger, this);
            if (!metadata.GetRequiredChanges(models, out creations, out migrations, out removals))
            {
                // Remove the backup and continue
                RemoveBackup(backupDatabase);
                return;
            }
            // Apply the changes
            try
            {
                BackupDatabase(connectionString, backupDatabase);
                MigrateTables(metadata, migrations);
                RemoveTables(metadata, removals);
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

        /// <summary>
        /// Get information about a table by it's name
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public TableInfo GetTableInfo(string table)
        {
            using (var conn = Open())
            {
                var results = conn.Select<TableInfo>(r => r.Table == table);
                if (results.Count == 1)
                    return results[0];
            }
            return null;
        }

        /// <summary>
        /// Get information about a table from the model type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public TableInfo GetTableInfo(Type model)
        {
            return GetTableInfo(model.GetModelMetadata().ModelName);
        }

        /// <summary>
        /// Get information about all tables
        /// </summary>
        /// <returns></returns>
        public List<TableInfo> GetTables()
        {
            using (var conn = Open())
            {
                return conn.Select<TableInfo>();
            }
        }
        #endregion
    }
}
