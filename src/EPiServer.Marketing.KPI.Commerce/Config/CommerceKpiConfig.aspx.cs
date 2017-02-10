using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI.WebControls;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Shell.WebForms;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Markets;

namespace EPiServer.Marketing.KPI.Commerce.Config
{
    [GuiPlugIn(Area = PlugInArea.AdminConfigMenu,
        UrlFromModuleFolder = "Admin/CommerceKpiConfig.aspx",
        LanguagePath = "/commercekpi/admin")]
    [ExcludeFromCodeCoverage]
    public partial class CommerceKpiConfig : WebFormsBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private IMarketService marketService = ServiceLocator.Current.GetInstance<IMarketService>();

        protected CommerceKpiSettings CommerceKpiSettings => CommerceKpiSettings.Current;

        protected override void OnInit(EventArgs e)
        {
            // Security validation: user should have Edit access to view this page
            if (!EPiServer.Security.PrincipalInfo.HasAdminAccess)
            {
                throw new AccessDeniedException();
            }
            
            var availableMarkets = marketService.GetAllMarkets();
            
            foreach (var market in availableMarkets)
            {
                PreferredMarketList.Items.Add(new ListItem(market.MarketName, market.MarketId.Value));
            }

            PreferredMarketList.SelectedValue = CommerceKpiSettings.Current.PreferredMarket.MarketId.Value;
           
            DataBind();
            base.OnInit(e);
        }

        protected void Save_OnClick(object sender, EventArgs e)
        {
            var settings = new CommerceKpiSettings()
            {
                PreferredMarket = marketService.GetMarket(PreferredMarketList.SelectedValue)
            };

            settings.Save();

            ShowMessage(string.Format(Translate("/abtesting/admin/success")), true);
        }

        protected void Cancel_OnClick(object sender, EventArgs e)
        {
            PreferredMarketList.SelectedValue = CommerceKpiSettings.PreferredMarket.MarketId.Value;
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
