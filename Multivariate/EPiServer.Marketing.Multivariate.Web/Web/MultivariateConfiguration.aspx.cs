using System;
using EPiServer.PlugIn;
using EPiServer.Shell.WebForms;


namespace EPiServer.Marketing.Multivariate.Web.Web
{
    [GuiPlugIn(Area = PlugInArea.AdminConfigMenu, UrlFromModuleFolder = "MultivariateWeb/MultivariateConfiguration.aspx", DisplayName = "Multivariate Test Configuration")]
    public partial class MultivariateConfiguration : WebFormsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}