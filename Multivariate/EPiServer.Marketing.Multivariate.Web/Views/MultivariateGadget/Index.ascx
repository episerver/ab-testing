<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Repositories" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.ServiceLocation" %>

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
                $('#btnRefresh')
                    .click(function () { epi.gadget.loadView(this, {'action':'Index/?id=1&amp;'});return false; });

            });
        </script>

<asp:Content>
	<div class="epi-buttonDefault">
	    <span class="epi-cmsButton">
        	    <input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Refresh" 
        		    type="button" id="btnRefresh" value="<%= LanguageManager.Instance.Translate("/multivariate/settings/refresh")%>" 
                    title="<%= LanguageManager.Instance.Translate("/multivariate/settings/refresh")%>" 
        		    onmouseover="EPi.ToolButton.MouseDownHandler(this)" 
        		    onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)">
	    </span>
    </div>

	<table class="epi-default">
	<tr>
	<tr>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/testend")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/state")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/winner")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/conversions")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/actions")%></th>
	</tr>	</tr>
	
	<%  
        UIHelper helper = new UIHelper();
        IMultivariateTestRepository repo = ServiceLocator.Current.GetInstance<IMultivariateTestRepository>();

        foreach (var item in Model) {
            var winner = repo.GetWinningTestResult(item);
		    int rate = 0;
		    if( winner.Views != 0 )
			    rate = (int)(winner.Conversions * 100.0 / winner.Views);
            %>
	<tr>
        <td><%= item.Title%></td>
        <td><%= item.StartDate%></td>
        <td><%= item.EndDate%></td>
        <td><%= item.testState%></td>
        <td><%= helper.getContent( item.OriginalItemId ).Name %></td>
        <td><%= helper.getContent( winner.ItemId ).Name %></td>
        <td><%= rate%>%</td>
        <td>
            <a length="0" class="epi-button-child-item" href="#" onclick="epi.gadget.loadView(this, {'action':'Details/<%=item.id%>'});return false;"
		        title="<%= LanguageManager.Instance.Translate("/multivariate/settings/details")%>">
                <img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/settings/details")%>" src="/App_Themes/Default/Images/Tools/Report.gif">
		    </a>
            <a href="<%= helper.getConfigurationURL()%>/Update/<%=item.id%>" title="<%= LanguageManager.Instance.Translate("/multivariate/settings/edit")%>">
                <img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/settings/edit")%>" src="/App_Themes/Default/Images/Tools/Edit.gif">
            </a>
	</td>                    
	</tr>
	<% } %>
    </table>
</asp:Content>