using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web
 {
    //Required by TestingStore.
    //Gets data from UI and transforms it to IMarketingTest data
    //to create and manage MarketingTests
    public class TestingStoreModel
    {
        public Guid testContentId { get; set; }
        public string testDescription { get; set; }
        public int publishedVersion { get; set; }
        public int variantVersion { get; set; }
        public string startDate { get; set; }
        public int testDuration { get; set; }
        public int participationPercent { get; set; }
        public int conversionPage { get; set; }
        public string testTitle { get; set; }

    }
}