<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Resources" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.ServiceLocation" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Repositories" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model.Enums" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
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
                $("td[colspan=9]").find("div").hide();
                $(".expand").click(function (event) {
                    event.stopPropagation();
                    var $target = $(event.target);
                    if ($target.closest("td").attr("colspan") > 1) {
                        $target.slideUp();
                        $target.closest("tr").prev().find("td:first").html("+");
                    } else {
                        $target.closest("tr").next().find("div").slideToggle();
                        if ($target.closest("tr").find("td:first").html() == "+")
                            $target.closest("tr").find("td:first").html("-");
                        else
                            $target.closest("tr").find("td:first").html("+");
                    }
                });
            });

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
<%  %>
    <div class="epi-contentContainer epi-padding">
        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= LanguageManager.Instance.Translate("/multivariate/settings/displayname")%>
                <a href="#" title="Help"><img class="EPEdit-CommandTool" align="absmiddle" border="0" alt="Help" src="/App_Themes/Default/Images/Tools/Help.png"/></a>
            </h1>
        </div>
        
	<div class="epi-buttonDefault">
	<span class="epi-cmsButton">
        	<input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Add" 
        		type="button" id="btnCreate" value="<%= LanguageManager.Instance.Translate("/multivariate/settings/create")%>" 
                title="<%= LanguageManager.Instance.Translate("/multivariate/settings/create")%>" 
        		onmouseover="EPi.ToolButton.MouseDownHandler(this)" 
        		onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)">
	</span>
    	</div>
    	
         <div>
            <table class="epi-default">
                <tr>
                    
                    <th colspan="2"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/testend")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/state")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage")%></th>
		            <th><%= LanguageManager.Instance.Translate("/multivariate/settings/variantpage")%></th>
                    <th> Stop Test</th>
                    <th> Delete </th>
                </tr>

                <%  foreach (var item in Model)
                    {   %>
                <tr>
                    <td class="expand" style="color: green; font-size: large;cursor: pointer">+</td>
                    <td>
                        <a href="<%: Url.Action("Edit", new {id = item.id}) %>"><%= item.Title %></a>
                    </td>
                    <td><%= item.StartDate %></td>
                    <td><%= item.EndDate %></td>
                    <td><%= item.testState %></td>
                    <td><%= item.OriginalItemId %></td>
                    <td><%= item.VariantItemId %></td>
                    <td style="text-align: center">
                        <% if (item.testState == TestState.Active)
                           { %>
                    	<a href="<%: Url.Action("Stop", new {id = item.id}) %>">
                    		<img border="0" 
                                alt="<%= LanguageManager.Instance.Translate("/multivariate/settings/delete") %>" 
                    		    src="./Images/StopTest.gif"
                                height="18"
                                width="18"
                    		    title="<%= LanguageManager.Instance.Translate("/multivariate/settings/delete") %>">
                    	</a>
                        <% } %> 
                    </td>
                    <td style="text-align:center">
                    	<a href="<%: Url.Action("Delete", new {id = item.id}) %>">
                    		<img border="0" 
                                alt="<%= LanguageManager.Instance.Translate("/multivariate/settings/delete")%>" 
                    		    src="/App_Themes/Default/Images/Tools/Delete.gif"
                    		    title="">
                    	</a>
                    </td>
                </tr>

                <tr>
                    <td colspan="9" class="detailsRow" style="padding: 0 0">
                        <div style="padding: 10px 10px">
                            <% Html.RenderPartial("_details",item); %>
                        
                        </div>
                    </td>
                   
                </tr>
                <% } %>
            </table>
        </div>

    </div>
</body>
</html>