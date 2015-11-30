using EPiServer.Core;

namespace Episerver.Multivariate.TestSite.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
