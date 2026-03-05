using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract
{
    public class SqlColumnMapping
    {
        /// <summary>
        /// Name of the column which you would like to map from.
        /// This is the name of the column in the underline SQL.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///  Name of the property which you would like to map to.
        ///  This is the name of the property in the class.
        /// </summary>
        public string Target { get; set; }

    }
}
