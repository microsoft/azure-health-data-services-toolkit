using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// 
    /// </summary>
    public class GenericRestClientOption : ClientOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public ServiceVersion Version { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        public GenericRestClientOption(ServiceVersion version = ServiceVersion.C)
        {
            Version = version;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ServiceVersion
        {
            /// <summary>
            /// 
            /// </summary>
            A = 1,
            /// <summary>
            /// 
            /// </summary>
            B = 2,
            /// <summary>
            /// 
            /// </summary>
            C = 3
        }
        /// <summary>
        /// 
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int IntProperty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public NestedOptions Nested { get; set; } = new NestedOptions();
        /// <summary>
        /// 
        /// </summary>
        public class NestedOptions
        {
            /// <summary>
            /// 
            /// </summary>
            public string Property { get; set; }
        }
    }
}

