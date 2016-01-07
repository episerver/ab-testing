<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>

<asp:Content>
<div class="epi-contentContainer epi-padding" >
<div class="epi-contentArea" >
<%= Html.ViewLinkButton(LanguageManager.Instance.Translate("/multivariate/settings/back"), LanguageManager.Instance.Translate("/multivariate/gadget/back"), "Index/?id=1&",  "", "", null)%>
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

</div>
</asp:Content>