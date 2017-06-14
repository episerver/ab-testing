using EPiServer.Shell.ViewComposition;

namespace EPiServer.Marketing.Testing.Web.Archived
{
    [Component(
        //Auto-plugs in the component to the assets panel of cms (See EPiServer.Shell.PlugInArea
        //in the EPiServer.UI assembly for Dashboard and CMS constants)
        PlugInAreas = "/episerver/cms/assets",
        Categories = "cms",
        WidgetType = "marketing-testing/cmsuicomponents/ArchivedTestComponent",
        //Define language path to translate Title/Description.
        LanguagePath = "/abtesting/archivedtestcomponent",
        Title = "Archived Tests",
        Description = "Shows archived tests for the select content",
        SortOrder = 1000)]
    public class ArchivedTestComponent
    {
    }
}
