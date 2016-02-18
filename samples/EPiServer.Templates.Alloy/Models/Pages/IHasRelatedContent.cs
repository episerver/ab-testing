using EPiServer.Core;

namespace EPiServer.Templates.Alloy.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
