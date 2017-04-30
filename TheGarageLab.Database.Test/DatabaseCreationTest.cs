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
    public class DatabaseCreationTest : TestBase
    {
        /// <summary>
        /// Constructor with output helper
        /// </summary>
        /// <param name="outputHelper"></param>
        public DatabaseCreationTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

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
            db.Create(dbfile);
            Assert.True(File.Exists(dbfile));
            Assert.Equal(0, db.GetTables().Count);
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
            db.Create(dbfile, typeof(SampleModels.ModelA));
            Assert.True(File.Exists(dbfile));
            Assert.Equal(1, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
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
            db.Create(dbfile, typeof(SampleModels.ModelA));
            Assert.True(File.Exists(dbfile));
            Assert.Equal(1, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.Null(db.GetTableInfo(typeof(SampleModels.ModelB)));
            // Recreate the database with a new model
            db = new Database(CreateLogger());
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
            db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB));
            Assert.True(File.Exists(dbfile));
            Assert.Equal(2, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelB)));
            // Recreate the database with a new model
            db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelA));
            Assert.Equal(1, db.GetTables().Count);
            Assert.NotNull(db.GetTableInfo(typeof(SampleModels.ModelA)));
            Assert.Null(db.GetTableInfo(typeof(SampleModels.ModelB)));
        }
    }
}
