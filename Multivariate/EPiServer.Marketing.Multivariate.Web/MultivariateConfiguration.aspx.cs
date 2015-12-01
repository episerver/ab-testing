using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;



namespace EPiServer.Marketing.Multivariate.Web
{
    [GuiPlugIn(Area = PlugInArea.AdminConfigMenu, UrlFromModuleFolder ="MultivariateConfiguration.aspx", DisplayName = "Multivariate Test Settings")]
    public partial class MultivariateConfiguration : WebFormsBase
    {
        IMultivariateTestRepository _multivariateTestRepository = new MultivariateTestRepository();

        protected override void OnLoad(EventArgs e)
        {
            List<IMultivariateTest> testList = _multivariateTestRepository.GetTestList(new MultivariateTestCriteria());


            Grid.DataSource = testList;
            Grid.DataBind();
        }


        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        protected void Create_Test(object sender, EventArgs e)
        {
            var startTime = Convert.ToDateTime(Request.Form["datetimestart"], CultureInfo.InvariantCulture);
            var endTime = Convert.ToDateTime(Request.Form["datetimeend"], CultureInfo.InvariantCulture);

            _multivariateTestRepository = new Repositories.MultivariateTestRepository();
            _multivariateTestRepository.CreateTest(TestTitle.Text, startTime, endTime, int.Parse(OriginPage.Text), 
                int.Parse(VariantPage.Text), int.Parse(ConversionPage.Text));

        }

        protected void Cancel_Create(object sender, EventArgs e)
        {
  

        }

       
    }

    
}