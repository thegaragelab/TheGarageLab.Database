using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGarageLab.Database.Test
{
    /// <summary>
    /// A set of sample models to use for testing
    /// </summary>
    public class SampleModels
    {
        public class ModelA
        {
            public static int VERSION = 1;

            public int Id { get; set; }

            public string Value { get; set; }
        }

        public class ModelA_V2
        {
            public static int VERSION = 2;

            public int Id { get; set; }

            public string Value { get; set; }

            public string Description { get; set; }
        }

        public class ModelB
        {
            public int Id { get; set; }

            public int Data { get; set; }
        }

        public class ModelB_V2
        {
            public static int VERSION = 2;

            public int Id { get; set; }

            public string Data { get; set; }
        }
    }
}
