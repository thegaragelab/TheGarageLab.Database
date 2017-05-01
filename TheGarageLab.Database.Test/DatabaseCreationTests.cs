using System;
using System.IO;
using ServiceStack.OrmLite;
using Xunit;
using Xunit.Abstractions;

namespace TheGarageLab.Database.Test
{
    /// <summary>
    /// Test database creation.
    /// </summary>
    public class DatabaseCreationTests : TestBase
    {
        /// <summary>
        /// Constructor with output helper
        /// </summary>
        /// <param name="outputHelper"></param>
        public DatabaseCreationTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        /// <summary>
        /// A database with no models is not much use but it
        /// will work.
        /// </summary>
        [Fact]
        public void CreateWithNoModelsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("CreateWithNoModelsWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile);
            Assert.True(File.Exists(dbfile));
            Assert.Equal(0, db.GetTables().Count);
        }

        /// <summary>
        /// Memory databases are not supported
        /// </summary>
        [Fact]
        public void CannotCreateMemoryDatabase()
        {
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            Assert.Throws<InvalidOperationException>(() => db.Create(Database.MEMORY_DATABASE));
        }

        /// <summary>
        /// Creating a database with models will work.
        /// </summary>
        [Fact]
        public void CreateWithModelsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("CreateWithModelsWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA));
            Assert.True(File.Exists(dbfile));
            Assert.Equal(1, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
        }

        /// <summary>
        /// If the database does not exist and the schema cannot be created
        /// no file will be created.
        /// </summary>
        [Fact]
        public void CreateWithInvalidModelWillNotCreateDatabase()
        {
            string dbfile = GetTestDatabaseFilename("CreateWithInvalidModelWillNotCreateDatabase.sqlite");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            Assert.ThrowsAny<Exception>(() => db.Create(dbfile, typeof(SampleModels.ModelB_Invalid)));
            Assert.False(File.Exists(dbfile));
        }

        /// <summary>
        /// Adding an additional model to an existing database will succeed.
        /// </summary>
        [Fact]
        public void AddingAdditionalModelsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("AddingAdditionalModelsWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            // Create the database with a single model
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA));
            Assert.True(File.Exists(dbfile));
            Assert.Equal(1, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.Null(db.GetTableInfo(typeof(SampleModels.ModelB)));
            // Recreate the database with a new model
            db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB));
            Assert.Equal(2, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelB)));
        }

        /// <summary>
        /// Removing a model from an existing database will succeed.
        /// </summary>
        [Fact]
        public void RemovingExistingModelsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("RemovingExistingModelsWillSucceed.sqlite");
            Assert.False(File.Exists(dbfile));
            // Create the database with a single model
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB));
            Assert.True(File.Exists(dbfile));
            Assert.Equal(2, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelB)));
            // Recreate the database with a new model
            db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA));
            Assert.Equal(1, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.Null(db.GetTableInfo(typeof(SampleModels.ModelB)));
        }

        /// <summary>
        /// Failure in schema change will not lose data.
        /// </summary>
        [Fact]
        public void MigrateWithInvalidModelWillNotLoseData()
        {
            string dbfile = GetTestDatabaseFilename("MigrateWithInvalidModelWillNotLoseData.sqlite");
            Assert.False(File.Exists(dbfile));
            // Create the database with two models
            IDatabase db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB));
            // Insert data in the database
            using (var conn = db.Open())
            {
                conn.Insert<SampleModels.ModelA>(new SampleModels.ModelA()
                {
                    Value = "Test"
                });
                conn.Insert<SampleModels.ModelB>(new SampleModels.ModelB()
                {
                    Data = 42
                });
            }
            // Try and upgrade to a faulty model
            db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            Assert.ThrowsAny<Exception>(() => db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB_Invalid)));
            // Ensure that data was not lost in the process
            db = new Database(CreateLogger());
            db.SqlLoggingSeverity = Logging.Severity.Debug;
            db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB));
            using (var conn = db.Open())
            {
                Assert.Equal(1, conn.Select<SampleModels.ModelA>().Count);
                Assert.Equal(1, conn.Select<SampleModels.ModelB>().Count);
            }
        }
    }
}
