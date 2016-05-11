using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Web.Models
{
    public class MarketingTestingContextModel
    {
        public IMarketingTest Test { get; set; }
        public int DaysElapsed { get; set; }
        public int DaysRemaining { get; set; }
        public string PublishedVersionName { get; set; }
        public string PublishedVersionPublishedBy { get; set; }
        public string PublishedVersionPublishedDate { get; set; }
        public string PublishedVersionContentLink { get; set; }



    }
}
