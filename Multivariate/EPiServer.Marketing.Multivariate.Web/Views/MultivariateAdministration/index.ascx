<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<EPiServer.Marketing.Multivariate.Model.IMultivariateTest>>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Resources" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model.Enums" %>
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
                $('td.parent')
                    .css("cursor", "pointer")
                    .attr("title", "Click to expand/collapse")
                    .click(function () {
                        $(this).parent.siblings('#child-' + this.id).toggle(); });
            });
            $(function () {
                $('#btnCreate')
                    .click(function () { location.href = '<%= Url.Action("Create","MultivariateAdministration") %>'; });

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
            <button id="btnCreate" type="button" class="epi-cmsButton-text epi-cmsButton-tools" Style="background:url('/App_Themes/Default/Images/General/addIcon.png');horiz-align:left;background-repeat: no-repeat">&nbsp Add Test</button>
        </div>
       <br/>
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
                    <td> Actions </td>
                </tr>

                <%  UIHelper helper = new UIHelper();
                    int index = 1;
                    foreach (var item in Model)
                    { %>
                <tr class="parent" id="parent<%= index %>">
                    <td><%= item.Title %></td>
                    <td><%= item.Owner %></td>
                    <td><%= item.TestState %></td>
                    <td><%= item.StartDate %></td>
                    <td><%= item.EndDate %></td>
                    <td><%= helper.getContent( item.OriginalItemId ).Name %></td>
                    <td></td>
                    <td>
                        <%= Html.ActionLink("Edit", "Update", new {id = item.Id})  %> |
                        <%= Html.ActionLink("Details", "GetAbTestById", new { id = item.Id })  %> |
                        <%= Html.ActionLink("Delete", "Delete", new {id = item.Id}) %>
                    </td>
                </tr>

                <tr  style="display: none">
                    <td id="child-parent<%= index %>" colspan="8">
                        <% if (item.TestState == 0)
                           { %>
                               <span style="color: red">This test has not been started</span>
                            <% } %>


                        <%  if (item.MultivariateTestResults != null)
                            {
                                foreach (var result in item.MultivariateTestResults)
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