<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<EPiServer.Marketing.Multivariate.IMultivariateTest>>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Resources" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
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


        <script>
            $(function () {
                $('tr.parent')
                    .css("cursor", "pointer")
                    .attr("title", "Click to expand/collapse")
                    .click(function () {
                        $(this).siblings('#child-' + this.id).toggle(); });
                });

            




        </script>


    </asp:PlaceHolder>


</head>
<body class="sleek">
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

                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/owner")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/state")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/testend")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/variantpage")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/conversionpage")%></th>
                </tr>

                <%  UIHelper helper = new UIHelper();
                    int index = 1;
                    foreach (var item in Model)
                    { %>
                <tr class="parent" id="parent<%= index %>">
                    <td><%= item.Title %></td>
                    <td><%= item.Owner %></td>
                    <td><%= item.State %></td>
                    <td><%= item.StartDate %></td>
                    <td><%= item.EndDate %></td>
                    <td><%= item.OriginalItemId %></td>


                </tr>

                <tr id="child-parent<%= index %>" style="display: none">
                    <td colspan="8">
                        <% if (item.State == TestState.Inactive)
                           { %>
                               <span style="color: red">This test has not been started</span>
                            <% } %>


                        <%  if (item.Results != null)
                            {
                                foreach (var result in item.Results)
                                { %>
                        <table>
                            <tr>
                                <th colspan="2"><%= result.ItemId %></th>
                            </tr>
                            <tr>
                                <th>Views</th>
                                <th>Conversions</th>
                            </tr>
                            <tr>
                                <td><%= result.Views %></td>
                                <td><%= result.Conversions %> </td>
                            </tr>
                        </table>
                        <% } %>
                        <% } %>  
                        
                    </td>
                </tr>
                <% index++;
                    } %>
            </table>
        </div>

    </div>
</body>
</html>
