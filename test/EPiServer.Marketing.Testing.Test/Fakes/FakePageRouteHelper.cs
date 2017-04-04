using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    public class FakePageRouteHelper : IPageRouteHelper
    {
        private PageData _fakePage;
        public FakePageRouteHelper(PageData fakeRequestedPage)
        {
            _fakePage = fakeRequestedPage;
        }
        public IContent Content
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ContentReference ContentLink
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string LanguageID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PageData Page
        {
            get
            {
                return _fakePage;
            }
        }

        public PageReference PageLink
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
