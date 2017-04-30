using ServiceStack.DataAnnotations;
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
    }
}
