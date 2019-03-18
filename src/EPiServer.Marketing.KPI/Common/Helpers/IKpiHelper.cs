using EPiServer.Core;

namespace EPiServer.Marketing.KPI.Common.Helpers
{
    public interface IKpiHelper
    {
        bool IsInSystemFolder();

        string GetUrl(ContentReference contentReference);

        string GetRequestPath();
    }
}
