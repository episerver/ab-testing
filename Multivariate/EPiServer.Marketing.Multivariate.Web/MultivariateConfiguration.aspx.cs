using System;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;



namespace EPiServer.Marketing.Multivariate.Web
{
    [GuiPlugIn(Area = PlugInArea.AdminConfigMenu, UrlFromModuleFolder ="MultivariateConfiguration.aspx", DisplayName = "Multivariate Test Settings")]
    public partial class MultivariateConfiguration : WebFormsBase
    {
        IMultivariateTestManager multivariateTestManager;
        IMultivariateTest multivariateTest;
        Repositories.IMultivariateTestRepository multivariateTestRepository;


        protected void Page_Load(object sender, EventArgs e)
        {
            var testTitle = FindControl("TestTitle");
        }

        protected void Create_Test(object sender, EventArgs e)
        {
            multivariateTestRepository = new Repositories.MultivariateTestRepository();
            multivariateTestRepository.CreateTest(TestTitle.Text, DateTime.Now, DateTime.Now,int.Parse(OriginPage.Text),int.Parse(VariantPage.Text),int.Parse(ConversionPage.Text));

        }

        protected void Cancel_Create(object sender, EventArgs e)
        {
  

        }
    }
}