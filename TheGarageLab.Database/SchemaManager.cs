using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using TheGarageLab.Logging;

namespace TheGarageLab.Database
{
    /// <summary>
    /// Helpers for managing schema and versions.
    /// </summary>
    internal class SchemaManager
    {
        /// <summary>
        /// The logger instance for output
        /// </summary>
        private readonly ILogger Logger;

        /// <summary>
        /// The database to use for operations
        /// </summary>
        private readonly IDatabase Database;

        /// <summary>
        /// Constructor with injections
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="database"></param>
        public SchemaManager(ILogger logger, IDatabase database)
        {
            Logger = logger;
            Database = database;
            // Make sure we have a metadata table
            using (var conn = Database.Open())
            {
                conn.CreateTableIfNotExists<SchemaMetadata>();
            }
        }

        #region Helpers
        /// <summary>
        /// Get the current version of the model.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private int GetModelVersion(Type t)
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
            return currentVersion;
        }

        /// <summary>
        /// Determine if migration is required for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool MigrationRequired(Type t)
        {
            int currentVersion = GetModelVersion(t);
            using (var conn = Database.Open())
            {
                // Get the version according to the schema record
                List<SchemaMetadata> tableInfo = conn.Select<SchemaMetadata>(x => (x.Table == t.GetModelMetadata().ModelName) && (x.Version != currentVersion));
                if (tableInfo.Count == 0)
                    return false;
                if (tableInfo[0].Version > currentVersion)
                    throw new InvalidOperationException("Table cannot be migrated to an older version.");
            }
            return true;
        }

        /// <summary>
        /// Determine if the table needs to be created
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool CreationRequired(Type t)
        {
            int currentVersion = GetModelVersion(t);
            using (var conn = Database.Open())
            {
                // Get the version according to the schema record
                List<SchemaMetadata> tableInfo = conn.Select<SchemaMetadata>(x => (x.Table == t.GetModelMetadata().ModelName) && (x.Version == currentVersion));
                return (tableInfo.Count == 0);
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Update the metadata for a single model
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMetadata(Type model)
        {
            // See if the model exists
            string tableName = model.GetModelMetadata().ModelName;
            using (var conn = Database.Open())
            {
                var modelList = conn.Select<SchemaMetadata>(conn.From<SchemaMetadata>().Where(r => r.Table == tableName));
                if (modelList.Count == 0)
                {
                    var metaData = new SchemaMetadata()
                    {
                        Table = tableName,
                        Version = GetModelVersion(model),
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow
                    };
                    conn.Insert<SchemaMetadata>(metaData);
                }
                else
                {
                    modelList[0].Modified = DateTime.UtcNow;
                    modelList[0].Version = GetModelVersion(model);
                    conn.Update(modelList[0]);
                }
            }
        }

        /// <summary>
        /// Determine what changes are required to bring the database
        /// up to date.
        /// </summary>
        /// <param name="models"></param>
        /// <param name="creations"></param>
        /// <param name="migrations"></param>
        /// <returns></returns>
        public bool GetRequiredChanges(Type[] models, out List<Type> creations, out List<Type> migrations)
        {
            // Set up the lists
            creations = new List<Type>();
            migrations = new List<Type>();
            // Determine what changes to make
            foreach (var model in models)
            {
                if (MigrationRequired(model))
                    migrations.Add(model);
                else if (CreationRequired(model))
                    creations.Add(model);
            }
            // Indicate if changes are required
            return (migrations.Any() || creations.Any());
        }
        #endregion
    }
}
