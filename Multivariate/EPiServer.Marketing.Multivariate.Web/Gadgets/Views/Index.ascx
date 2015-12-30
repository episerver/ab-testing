<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>

<asp:Content>
<p>&nbsp;&nbsp;<%= Html.ViewLinkButton(LanguageManager.Instance.Translate("/multivariate/gadget/refresh"), LanguageManager.Instance.Translate("/multivariate/gadget/refresh"), "Index/?id=1&",  "", "", null)%></p>
	<table class="epi-default">
	<tr>
	<tr>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/name")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/start")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/finish")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/state")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/original")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/winner")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/conversions")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/actions")%></th>
	</tr>	</tr>
	
	<%  UIHelper helper = new UIHelper();
        foreach (var item in Model) { %>
	<tr>
        <td><%= item.Title%></td>
        <td><%= item.StartDate%></td>
        <td><%= item.EndDate%></td>
        <td><%= item.TestState%></td>
        <td><%= helper.getContent( item.OriginalItemId ).Name %></td>
        <td></td>
        <td>58%</td>
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