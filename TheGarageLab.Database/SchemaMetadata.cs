using System;
using System.Data;
using ServiceStack.DataAnnotations;

namespace TheGarageLab.Database
{
    /// <summary>
    /// This class provides information about the curent database
    /// schema. It is used to manage migrations.
    /// </summary>
    internal class SchemaMetadata
    {
        /// <summary>
        /// The name of the table
        /// </summary>
        [PrimaryKey]
        public string Table { get; set; }

        /// <summary>
        /// The current version of the table
        /// </summary>
        [Required]
        public int Version { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        [Required]
        public DateTime Created { get; set; }

        /// <summary>
        /// Modification timestamp
        /// </summary>
        [Required]
        public DateTime Modified { get; set; }
    }
}
