using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Common.Helpers
{
    interface IKpiEpiserverHelper
    {
        /// <summary>
        /// Leverages UrlResolver to return the url of content associated with the supplied content reference        /// 
        /// </summary>
        /// <param name="contentReference"></param>
        /// <returns></returns>
        string GetUrl(ContentReference contentReference);
    }
}
