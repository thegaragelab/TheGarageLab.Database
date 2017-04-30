using System;
using System.IO;
using TheGarageLab.Logging;
using Xunit.Abstractions;

namespace TheGarageLab.Database.Test
{
    /// <summary>
    /// Base class for unit tests
    /// </summary>
    public class TestBase
    {
        /// <summary>
        /// The output helper
        /// </summary>
        private readonly ITestOutputHelper OutputHelper;

        /// <summary>
        /// Logger implementation using the xUnit output stream.
        /// </summary>
        private class TestLogger : ILogger
        {
            /// <summary>
            /// Output helper provided by xUnit
            /// </summary>
            private readonly ITestOutputHelper OutputHelper;

            public TestLogger(ITestOutputHelper outputHelper)
            {
                OutputHelper = outputHelper;
            }

            /// <summary>
            /// Write an output message
            /// </summary>
            /// <param name="severity"></param>
            /// <param name="message"></param>
            /// <param name="cause"></param>
            public void Write(Severity severity, string message, Exception cause = null)
            {
                OutputHelper.WriteLine($"{severity}: {message}");
                if (cause != null)
                    OutputHelper.WriteLine(cause.ToString());
            }
        }

        /// <summary>
        /// Constructor with an output helper
        /// </summary>
        /// <param name="outputHelper"></param>
        public TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Create a logger using the test output.
        /// </summary>
        /// <returns></returns>
        public ILogger CreateLogger()
        {
            return new TestLogger(OutputHelper);
        }

        /// <summary>
        /// Get the full name of a test database file and optionally
        /// delete it if it exists.
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public string GetTestDatabaseFilename(string baseName, bool remove = true)
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseName);
            if (remove)
                File.Delete(filename);
            return filename;
        }
    }
}
