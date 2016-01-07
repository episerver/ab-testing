<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<EPiServer.Marketing.Multivariate.Model.IMultivariateTest>>" %>
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

                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/testend")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/state")%></th>
                    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage")%></th>
		    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/winner")%></th>
		    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/conversions")%></th>
		    <th><%= LanguageManager.Instance.Translate("/multivariate/settings/actions")%></th>
                </tr>

                <%  UIHelper helper = new UIHelper();
        	    IMultivariateTestRepository repo = ServiceLocator.Current.GetInstance<IMultivariateTestRepository>();
                
                    int index = 1;
                    foreach (var item in Model)
                    { 
			var winner = repo.GetWinningTestResult(item);
			int rate = 0;
			if( winner.Views != 0 )
				rate = (int)(winner.Conversions * 100.0 / winner.Views);
                    
		%>
                <tr class="parent" id="parent<%= index %>">
                    <td><a href="<%: Url.Action("Update", new {id = item.Id}) %>"><%= item.Title %></a></td>
                    <td><%= item.StartDate %></td>
                    <td><%= item.EndDate %></td>
                    <td><%= item.TestState %></td>
                    <td><%= helper.getContent( item.OriginalItemId ).Name %></td>
                    <td><%= helper.getContent( winner.ItemId ).Name %></td>
                    <td><%= rate%>%</td>
                    <td>
                    	<a href="<%: Url.Action("Details", new {id = item.Id}) %>">
                    		<img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/gadget/details")%>" 
                    		src="/App_Themes/Default/Images/Tools/Report.gif"
                    		title="<%= LanguageManager.Instance.Translate("/multivariate/settings/details")%>">
                    	</a>
                    	<a href="<%: Url.Action("Update", new {id = item.Id}) %>">
                    		<img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/settings/edit")%>" 
                    		src="/App_Themes/Default/Images/Tools/Edit.gif" 
                    		title="<%= LanguageManager.Instance.Translate("/multivariate/settings/edit")%>">
                    	</a>
                    	<a href="<%: Url.Action("Delete", new {id = item.Id}) %>">
                    		<img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/settings/delete")%>" 
                    		src="/App_Themes/Default/Images/Tools/Delete.gif"
                    		title="<%= LanguageManager.Instance.Translate("/multivariate/settings/delete")%>">
                    	</a>
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