<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>

<asp:Content>
<div class="epi-contentContainer epi-padding" >
<div class="epi-contentArea" >
<%= Html.ViewLinkButton(LanguageManager.Instance.Translate("/multivariate/gadget/back"), LanguageManager.Instance.Translate("/multivariate/gadget/back"), "Index/?id=1&",  "", "", null)%>
<h1 class="EP-prefix"><%= LanguageManager.Instance.Translate("/multivariate/gadget/details")%></h1>
</div>
<table>
<%  	
	UIHelper helper = new UIHelper();
	foreach (var item in Model) { 
%>
	<tr><td>Name</td><td><%= item.Title%></td><td>&nbsp;&nbsp;State</td><td>&nbsp;&nbsp;<%= item.TestState%></td></tr>
	<tr><td>Owner</td><td><%= item.Owner%></td><td>&nbsp;&nbsp;Start</td><td>&nbsp;&nbsp;<%= item.StartDate%></td></tr>
	<tr><td>&nbsp;&nbsp;</td><td>&nbsp;&nbsp;</td><td>&nbsp;&nbsp;End</td><td>&nbsp;&nbsp;<%= item.EndDate%></td></tr>
<tr><td>&nbsp;&nbsp;</td></tr>
	<tr><td>Original Item</td><td>&nbsp;&nbsp;<%= helper.getContent( item.OriginalItemId ).Name %></td><td>&nbsp;&nbsp;</td><td>&nbsp;&nbsp;</td></tr>
</table>
<br>
<div class="epi-contentArea" >
<h1><%= LanguageManager.Instance.Translate("/multivariate/gadget/results")%></h1>
</div>

<% } %>

</div>
</asp:Content>
