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
            string dbfile = GetTestDatabaseFilename("CreateWithNoModelsWillSucceed.db");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.Create(dbfile);
            Assert.True(File.Exists(dbfile));
        }

        /// <summary>
        /// Creating a database with models will work.
        /// </summary>
        [Fact]
        public void CreateWithModelsWillSucceed()
        {
            string dbfile = GetTestDatabaseFilename("CreateWithModelsWillSucceed.db");
            Assert.False(File.Exists(dbfile));
            IDatabase db = new Database(CreateLogger());
            db.Create(dbfile, typeof(SampleModels.ModelA), typeof(SampleModels.ModelB));
            Assert.True(File.Exists(dbfile));
            // TODO: Access to some metadata would be handy, in the meantime
            //       we will just make sure we can add entries.
            using (var conn = db.Open())
            {
                var id = conn.Insert<SampleModels.ModelA>(new SampleModels.ModelA());
                Assert.Equal(1, conn.Select<SampleModels.ModelA>().Count);
                id = conn.Insert<SampleModels.ModelB>(new SampleModels.ModelB());
                Assert.Equal(1, conn.Select<SampleModels.ModelB>().Count);
            }
        }

        /// <summary>
        /// Adding an additional model to an existing database will succeed.
        /// </summary>
        [Fact]
        public void AddingAdditionalModelsWillSucceed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removing a model from an existing database will succeed.
        /// </summary>
        [Fact]
        public void RemovingExistingModelsWillSucceed()
        {
            throw new NotImplementedException();
        }
    }
}
