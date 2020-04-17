using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI.WebControls;

namespace EPiServer.Marketing.Testing.Web.Config
{
    [GuiPlugIn(Area = PlugInArea.AdminConfigMenu,
        UrlFromModuleFolder = "Admin/AdminConfig.aspx",
        LanguagePath = "/abtesting/admin")]
    [ExcludeFromCodeCoverage]
    public partial class AdminConfig : WebFormsBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        protected AdminConfigTestSettings TestSettings => new AdminConfigTestSettings()
        {
            TestDuration = AdminConfigTestSettings.Current.TestDuration,
            ParticipationPercent = AdminConfigTestSettings.Current.ParticipationPercent,
            ConfidenceLevel = AdminConfigTestSettings.Current.ConfidenceLevel,
            AutoPublishWinner = AdminConfigTestSettings.Current.AutoPublishWinner,
            IsEnabled = AdminConfigTestSettings.Current.IsEnabled
        };


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
            var duration = 0;
            var particiaption = 0;

            if (!InputIsValid(TestDuration.Text, out duration) || !InputIsValid(ParticipationPercent.Text, out particiaption))
            {
                ShowMessage(string.Format(Translate("/abtesting/admin/invalid")), false);
                return;
            }

            var returnError = false;
            if (particiaption < 1 || particiaption > 100)
            {
                ShowMessage(string.Format(Translate("/abtesting/admin/participationerror")), false);
                returnError = true;
            }

            if (duration < 1 || duration > 365)
            {

                ShowMessage(string.Format(Translate("/abtesting/admin/durationerror")), false);
                returnError = true;
            }

            if (returnError)
            {
                return;
            }

            var newSettings = new AdminConfigTestSettings()
            {
                TestDuration = duration,
                ParticipationPercent = particiaption,
                ConfidenceLevel = Convert.ToInt16(ConfidenceLevel.SelectedValue),
                AutoPublishWinner = Convert.ToBoolean(AutoPublishWinner.SelectedValue),
                IsEnabled = chkIsEnabled.Checked
            };

            newSettings.Save();
            ShowMessage(string.Format(Translate("/abtesting/admin/success")), true);
        }

        private bool InputIsValid(string value, out int convertedValue)
        {
            convertedValue = 0;

            try
            {
                convertedValue = Convert.ToInt32(value);
            }
            catch (OverflowException)
            {
                _logger.Error("{0} is outside the range of the Int32 type.", value);
                return false;
            }
            catch (FormatException)
            {
                _logger.Error("The {0} value '{1}' is not in a recognizable format.",
                    value.GetType().Name, value);
                return false;
            }

            return true;
        }

        protected void Cancel_OnClick(object sender, EventArgs e)
        {
            TestDuration.Text = TestSettings.TestDuration.ToString();
            ParticipationPercent.Text = TestSettings.ParticipationPercent.ToString();
            ConfidenceLevel.Text = TestSettings.ConfidenceLevel.ToString();
            AutoPublishWinner.SelectedValue = TestSettings.AutoPublishWinner.ToString();
            chkIsEnabled.Checked = TestSettings.IsEnabled;
        }

        private void ShowMessage(string msg, bool isWarning)
        {
            var message = new Panel
            {
                CssClass = isWarning ? "EP-systemMessage EP-systemMessage-Warning" : "EP-systemMessage"
            };

            message.Controls.Add(new Literal { Text = msg });
            MessagePanel.Controls.Add(message);
            MessagePanel.Visible = true;
        }
    }
}
