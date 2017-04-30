using System;
using System.IO;
using ServiceStack.OrmLite;
using Xunit;
using Xunit.Abstractions;

namespace TheGarageLab.Database.Test
{
    /// <summary>
    /// Test migration operations
    /// </summary>
    public class DatabaseMigrationTests : TestBase
    {
        /// <summary>
        /// Constructor with output helper
        /// </summary>
        /// <param name="outputHelper"></param>
        public DatabaseMigrationTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        /// <summary>
        /// Migrating a model that adds fields will succeed
        /// </summary>
        [Fact]
        public void MigrateModelWithAdditionalFieldsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("MigrateModelWithAdditionalFieldsWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelA));
            // Insert a sample record
            using (var conn = db.Open())
            {
                conn.Insert<SampleModels.ModelA>(new SampleModels.ModelA()
                {
                    Value = "Test"
                });
            }
            // Migrate to new schema
            db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelA_V2));
            // Ensure the model has been updated
            Assert.Equal(SampleModels.ModelA_V2.VERSION, db.GetTableInfo(typeof(SampleModels.ModelA_V2)).Version);
            // Ensure the data is still present
            using (var conn = db.Open())
            {
                var records = conn.Select<SampleModels.ModelA_V2>();
                Assert.Equal(1, records.Count);
                Assert.Equal("Test", records[0].Value);
                Assert.Null(records[0].Description);
            }
        }

        /// <summary>
        /// Migrating a model that removes fields will succeed
        /// </summary>
        [Fact]
        public void MigrateModelWithRemovedFieldsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("MigrateModelWithRemovedFieldsWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelC));
            // Insert a sample record
            using (var conn = db.Open())
            {
                conn.Insert<SampleModels.ModelC>(new SampleModels.ModelC()
                {
                    Value = "Value",
                    Description = "Description"
                });
            }
            // Migrate to new schema
            db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelC_V2));
            // Ensure the model has been updated
            Assert.Equal(SampleModels.ModelC_V2.VERSION, db.GetTableInfo(typeof(SampleModels.ModelC_V2)).Version);
            // Ensure the data is still present
            using (var conn = db.Open())
            {
                var records = conn.Select<SampleModels.ModelC_V2>();
                Assert.Equal(1, records.Count);
                Assert.Equal("Value", records[0].Value);
            }
        }

        /// <summary>
        /// Migrating a model that changes field types will succeed
        /// </summary>
        [Fact]
        public void MigrateModelWithDifferentFieldTypesWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("MigrateModelWithDifferentFieldTypesWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelB));
            // Insert a sample record
            using (var conn = db.Open())
            {
                conn.Insert<SampleModels.ModelB>(new SampleModels.ModelB()
                {
                    Data = 42
                });
            }
            // Migrate to new schema
            db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelB_V2));
            // Ensure the model has been updated
            Assert.Equal(SampleModels.ModelB_V2.VERSION, db.GetTableInfo(typeof(SampleModels.ModelB_V2)).Version);
            // Ensure the data is still present
            using (var conn = db.Open())
            {
                var records = conn.Select<SampleModels.ModelB_V2>();
                Assert.Equal(1, records.Count);
                Assert.Equal("42", records[0].Data);
            }
        }

        /// <summary>
        /// Migration can only occur upwards, cannot revert to earlier
        /// version.
        /// </summary>
        [Fact]
        public void CannotMigrateTableToLowerVersion()
        {
            string dbfile = GetTestDatabaseFilename("CannotMigrateTableToLowerVersion.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelB_V2));
            // Migrate to older schema
            db = new Database(CreateLogger());
            Assert.Throws<InvalidOperationException>(() => db.Create(dbfile, typeof(SampleModels.ModelB)));
        }
    }
}
