using ServiceStack.DataAnnotations;
using System.Data;

namespace TheGarageLab.Database.Test
{
    /// <summary>
    /// A set of sample models to use for testing
    /// </summary>
    public class SampleModels
    {
        [Alias("ModelA")]
        public class ModelA
        {
            public static int VERSION = 1;

            public int Id { get; set; }

            public string Value { get; set; }
        }

        [Alias("ModelA")]
        public class ModelA_V2
        {
            public static int VERSION = 2;

            public int Id { get; set; }

            public string Value { get; set; }

            public string Description { get; set; }
        }

        [Alias("ModelB")]
        public class ModelB
        {
            public int Id { get; set; }

            public int Data { get; set; }
        }

        [Alias("ModelB")]
        public class ModelB_V2
        {
            public static int VERSION = 2;

            public int Id { get; set; }

            public string Data { get; set; }
        }

        [Alias("ModelB")]
        public class ModelB_Invalid
        {
            public static int VERSION = 3;
        }

        [Alias("ModelC")]
        public class ModelC
        {
            public static int VERSION = 1;

            public int Id { get; set; }

            public string Value { get; set; }

            public string Description { get; set; }
        }

        [Alias("ModelC")]
        public class ModelC_V2
        {
            public static int VERSION = 2;

            public int Id { get; set; }

            public string Value { get; set; }
        }

        [Alias("ModelD")]
        public class ModelD
        {
            public static int VERSION = 1;

            public int Id { get; set; }

            public string Value { get; set; }

            public string Description { get; set; }
        }

        [Alias("ModelD")]
        public class ModelD_V2 : DefaultMigrator
        {
            public static int VERSION = 2;

            public int Id { get; set; }

            public string Value { get; set; }

            /// <summary>
            /// Migrate a single record.
            /// </summary>
            /// <param name="record"></param>
            /// <returns></returns>
            public override object MigrateRecord(IDataRecord record)
            {
                // Do a standard migration
                var result = base.MigrateRecord(record) as ModelD_V2;
                // Make the value a combination of value and description
                var description = record.GetValue(record.GetOrdinal("Description")).ToString();
                result.Value = $"{result.Value} ({description})";
                // All done
                return result;
            }
        }
    }
}
