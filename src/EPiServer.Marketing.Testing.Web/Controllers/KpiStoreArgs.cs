using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    public class KpiStoreArgs
    {
        /// <summary>
        /// Kpi Type information as a string
        /// </summary>
        public string KpiType { get; set; }
        /// <summary>
        /// Data bundled from the UI and returned as a JSon string.
        /// </summary>
        public string KpiJsonFormData { get; set; }
    }
}
