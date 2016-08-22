using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.PlugIn;
using EPiServer.Shell.WebForms;

namespace EPiServer.Marketing.Testing.Web.Config
{
    [GuiPlugIn(Area = PlugInArea.AdminConfigMenu,
        UrlFromModuleFolder = "Admin/AdminConfig.aspx",
        LanguagePath = "/abtesting/admin")]
    public partial class AdminConfig : WebFormsBase
    {
        protected AdminConfigTestSettings TestSettings => AdminConfigTestSettings.Current;

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

        protected void Save_OnClick(object sender, EventArgs e)
        {

            var settings = new AdminConfigTestSettings()
            {
                TestDuration = Convert.ToInt32(TestDuration.Text),
                ParticipationPercent = Convert.ToInt32(ParticipationPercent.Text),
                ConfidenceLevel = Convert.ToInt32(ConfidenceLevel.Text),
            };

            settings.Save();
        }

        protected void Cancel_OnClick(object sender, EventArgs e)
        {
            TestDuration.Text = TestSettings.TestDuration.ToString();
            ParticipationPercent.Text = TestSettings.ParticipationPercent.ToString();
            ConfidenceLevel.Text = TestSettings.ConfidenceLevel.ToString();
        }
    }
}
