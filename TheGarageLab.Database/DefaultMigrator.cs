using System;
using System.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;

namespace TheGarageLab.Database
{
    /// <summary>
    /// Generic migrator. This implementation uses a brute
    /// force approach to migrate the table (which is generally
    /// good enough for simple changes).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultMigrator : IMigrator
    {
        /// <summary>
        /// The model type we are working with
        /// </summary>
        private Type ModelType;

        /// <summary>
        /// Map database field names to model definitions
        /// </summary>
        private Dictionary<string, FieldDefinition> Fields;

        /// <summary>
        /// Prepare for migration of a set of records
        /// </summary>
        /// <param name="fromVersion"></param>
        /// <param name="model"></param>
        public void BeginMigration(int fromVersion, Type model)
        {
            ModelType = model;
            // Create the field cache for faster lookup
            Fields = new Dictionary<string, FieldDefinition>();
            foreach (var fieldInfo in ModelType.GetModelMetadata().FieldDefinitions)
                Fields[fieldInfo.Name] = fieldInfo;
        }

        /// <summary>
        /// Migrate a single record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public object MigrateRecord(IDataRecord record)
        {
            // Create the new record instance
            var instance = Activator.CreateInstance(ModelType);
            // If we have matching data in the source record set the field
            for (int i = 0; i < record.FieldCount; i++)
            {
                // Do we have a matching field?
                FieldDefinition fieldInfo;
                if (!Fields.TryGetValue(record.GetName(i), out fieldInfo))
                    continue;
                // Set the (coerced) value
                fieldInfo.PropertyInfo.SetValue(
                    instance,
                    Convert.ChangeType(
                        record.GetValue(i),
                        fieldInfo.PropertyInfo.PropertyType
                        )
                    );
            }
            // All done
            return instance;
        }
    }
}
