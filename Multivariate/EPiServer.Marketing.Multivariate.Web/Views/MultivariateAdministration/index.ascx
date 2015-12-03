<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<EPiServer.Marketing.Multivariate.IMultivariateTest>>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Resources" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>

<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title></title>
    <asp:PlaceHolder runat="server">
        <%=Page.ClientResources("ShellCore")%>
        <%=Page.ClientResources("ShellCoreLightTheme")%>
        <%= Html.ScriptResource(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/BrokenLinks/BrokenLinks.js"))%>
        <%= Html.CssLink(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/BrokenLinks/BrokenLinks.css"))%>
        <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "system.css"))%>
        <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "ToolButton.css"))%>
        <%= Html.ScriptResource(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/ReportCenter/ReportCenter.js"))%>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUtilBySettings("javascript/episerverscriptmanager.js"))%>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.js")) %>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/dialog.js")) %>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.aspx")) %>

        <link href="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/themes/smoothness/jquery-ui.min.css" rel="stylesheet">
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js"></script>

        <link rel="stylesheet" type="text/css" href="../Scripts/datetimepicker/jquery.datetimepicker.css" />
        <script src="../Scripts/datetimepicker/jquery.js"></script>
        <script src="../Scripts/datetimepicker/jquery.datetimepicker.full.js"></script>
        <script>
           
        </script>


    </asp:PlaceHolder>


</head>
<body>
    <div class="epi-contentContainer epi-padding">
        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= LanguageManager.Instance.Translate("/multivariate/settings/displayname")%>
            </h1>
        </div>
        <div>
            <%= Html.ActionLink("Create New Test","Create",null,null) %>
        </div>
        <div>
            <table class="epi-default">
                <tr>

                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/owner")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/state")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/testend")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/variantpage")%></th>
                    <th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/conversionpage")%></th>
                </tr>

                <%  UIHelper helper = new UIHelper();
                    foreach (var item in Model)
                    { %>
                <tr>
                    <td><%= item.Title %></td>
                    <td><%= item.Owner %></td>
                    <td><%= item.State %></td>
                    <td><%= item.StartDate %></td>
                    <td><%= item.EndDate %></td>
                    <td><%= item.OriginalItemId %></td>
                    <td><%= item.VariantItemId %></td>
                    <td><%= item.ConversionItemId %></td>
                 

                </tr>
                <% } %>
            </table>
        </div>

    </div>
</body>
</html>
