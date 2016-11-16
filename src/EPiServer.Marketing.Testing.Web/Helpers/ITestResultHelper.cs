using EPiServer.Core;
using EPiServer.DataAccess;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    internal interface ITestResultHelper
    {
        IContent GetClonedContentFromReference(ContentReference reference);
        ContentReference PublishContent(IContent contentToPublish);

    }
}
