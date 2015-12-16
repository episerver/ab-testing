using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.Multivariate.Model;
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
            var startDate = Request.Form["datetimestart"];
            var endDate = Request.Form["datetimeend"];

            if (string.IsNullOrEmpty(startDate) || startDate == Translate("/multivariate/settings/startdate"))
            {
                // TODO: display message saying start date is not set
                
                return;
            }
            else if (string.IsNullOrEmpty(endDate) || endDate == Translate("/multivariate/settings/endDate"))
            {
                // TODO: display message saying end date is not set

                return;
            }

            var startTime = Convert.ToDateTime(startDate, CultureInfo.InvariantCulture);
            var endTime = Convert.ToDateTime(endDate, CultureInfo.InvariantCulture);

            _multivariateTestRepository = new Repositories.MultivariateTestRepository();
            _multivariateTestRepository.CreateTest(TestTitle.Text, startTime, endTime, int.Parse(OriginPage.Text), 
                int.Parse(VariantPage.Text), int.Parse(ConversionPage.Text));

        }

        protected void Cancel_Create(object sender, EventArgs e)
        {
  

        }


        /// <summary>
        /// Check for Edit Access before displaying this page or generating Registration Script
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            // Security validation: user should have Edit access to view this page
            if (!EPiServer.Security.PrincipalInfo.HasAdminAccess)
            {
                throw new AccessDeniedException();
            }

            DataBind();
            base.OnInit(e);
        }


    }

    
}