using EPiServer.Core;
using System;

namespace EPiServer.Marketing.Multivariate.Web.Helpers
{
    public interface IUIHelper
    {
        IContent getContent(Guid guid);
        String getConfigurationURL();
    }
}
