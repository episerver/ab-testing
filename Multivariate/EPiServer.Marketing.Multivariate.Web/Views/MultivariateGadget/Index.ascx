<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Repositories" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.ServiceLocation" %>

<asp:Content>
<p>&nbsp;&nbsp;<%= Html.ViewLinkButton(LanguageManager.Instance.Translate("/multivariate/gadget/refresh"), LanguageManager.Instance.Translate("/multivariate/gadget/refresh"), "Index/?id=1&",  "", "", null)%></p>
	<table class="epi-default">
	<tr>
	<tr>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/testend")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/state")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/winner")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/conversions")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/actions")%></th>
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
        <td><%= item.TestState%></td>
        <td><%= helper.getContent( item.OriginalItemId ).Name %></td>
        <td><%= helper.getContent( winner.ItemId ).Name %></td>
        <td><%= rate%>%</td>
        <td>
            <a length="0" class="epi-button-child-item" href="#" onclick="epi.gadget.loadView(this, {'action':'Details/?id=<%=item.Id%>&'});return false;"
		        title="<%= LanguageManager.Instance.Translate("/multivariate/gadget/details")%>">
                <img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/gadget/details")%>" src="/App_Themes/Default/Images/AdminMenu/Settings.gif">
		    </a>
            <a href="<%= helper.getConfigurationURL()%>/Update/<%=item.Id%>" title="<%= LanguageManager.Instance.Translate("/multivariate/gadget/edit")%>">
                <img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/gadget/edit")%>" src="/App_Themes/Default/Images/AdminMenu/ToolSettings.gif">
            </a>
            <a href="" title="<%= LanguageManager.Instance.Translate("/multivariate/gadget/promote")%>">
                <img border="0" alt="<%= LanguageManager.Instance.Translate("/multivariate/gadget/promote")%>" src="/App_Themes/Default/Images/Tools/Up.gif">
            </a>
	</td>                    
	</tr>
	<% } %>
    </table>
</asp:Content>