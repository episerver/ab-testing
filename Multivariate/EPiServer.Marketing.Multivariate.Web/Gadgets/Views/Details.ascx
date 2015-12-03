<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
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
	<tr><td>Name</td><td><%= item.Title%></td><td>&nbsp;&nbsp;State</td><td>&nbsp;&nbsp;<%= item.State%></td></tr>
	<tr><td>Owner</td><td><%= item.Owner%></td><td>&nbsp;&nbsp;Start</td><td>&nbsp;&nbsp;<%= item.StartDate%></td></tr>
	<tr><td>&nbsp;&nbsp;</td><td>&nbsp;&nbsp;</td><td>&nbsp;&nbsp;End</td><td>&nbsp;&nbsp;<%= item.EndDate%></td></tr>
<tr><td>&nbsp;&nbsp;</td></tr>
	<tr><td>Original Item</td><td><%= helper.getContent( item.OriginalItemId ).Name %></td><td>&nbsp;&nbsp;</td><td>&nbsp;&nbsp;</td></tr>
	<tr><td>Conversion Item&nbsp;&nbsp;</td><td><%= helper.getContent( item.ConversionItemId).Name %></td></tr>
	<tr><td>Varient Item</td><td><%= helper.getContent( item.VariantItemId ).Name %></td></tr>
</table>
<br>
<div class="epi-contentArea" >
<h1><%= LanguageManager.Instance.Translate("/multivariate/gadget/results")%></h1>
</div>
<table class="epi-default">
	<tr>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/name")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/views")%></th>
		<th class="episize300"><%= LanguageManager.Instance.Translate("/multivariate/gadget/resultconversions")%></th>
	</tr>

<%  	
	foreach (var result in item.Results) { 
%>
	<tr>
	<td><%= helper.getContent( result.ItemId ).Name %></td><td><%= result.Views  %></td><td><%= result.Conversions%></td>
	</tr>
<% 	} %>
</div>
</asp:Content>
<% } %>
