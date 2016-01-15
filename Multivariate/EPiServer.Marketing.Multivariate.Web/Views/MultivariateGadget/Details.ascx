<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>

    <%=Page.ClientResources("ShellCoreLightTheme")%>
    <%= Html.ScriptResource(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/BrokenLinks/BrokenLinks.js"))%>
        
    <%= Html.CssLink(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/BrokenLinks/BrokenLinks.css"))%>
    <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "system.css"))%>
    <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "ToolButton.css"))%>
        
    <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.js")) %>
    <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/dialog.js")) %>

        <script>
            $(function () {
                $('#btnBack')
                    .click(function () { epi.gadget.loadView(this, {'action':'Index/?id=1&amp;'});return false; });

            });
        </script>

<asp:Content>
<div class="epi-contentArea " >
	<div class="epi-buttonDefault">
	    <span class="epi-cmsButton">
		    <input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-ArrowLeft" 
			    type="button" id="btnBack" value="<%= LanguageManager.Instance.Translate("/multivariate/settings/back")%>" 
		    title="<%= LanguageManager.Instance.Translate("/multivariate/settings/back")%>" 
			    onmouseover="EPi.ToolButton.MouseDownHandler(this)" 
			    onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)">
	    </span>
	</div>
<h1 class="EP-prefix"><%= LanguageManager.Instance.Translate("/multivariate/settings/details")%></h1>
<table>
<tr><th>Name</th><th>Owner</th><th>State</th><th>Start</th><th>End</th></tr>
<%  	
	UIHelper helper = new UIHelper();
	foreach (var item in Model) { 
%>
	<tr><td><%= item.Title%></td><td><%= item.Owner%></td><td><%= item.testState%></td><td><%= item.StartDate%></td><td><%= item.EndDate%></tr>
</table>
</div>
<div class="epi-contentArea" >
<h1><%= LanguageManager.Instance.Translate("/multivariate/settings/results")%></h1>
<table>
<tr><th>Item name</th><th>Views</th><th>Conversions</th><th>Conversion Rate</th></tr>
<%
	foreach( var result in item.TestResults ) {
		int rate = 0;
		if( result.Views != 0 )
			rate = (int)(result.Conversions * 100.0 / result.Views);
%>
	<tr><td><%= helper.getContent( result.ItemId ).Name %></td><td><%= result.Views %></td><td><%= result.Conversions %></td><td><%= rate %> %</td></tr>
	
	<% } %>
</table>
</div>

<% } %>

</asp:Content>
