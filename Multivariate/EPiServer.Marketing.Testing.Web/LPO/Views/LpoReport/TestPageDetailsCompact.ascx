<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.TestPageData>" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<div class="epi-LPOGadget-itemCompact<%= Model.IsWinner ? " epi-LPOGadget-item-winner" : String.Empty %>">
    <div class="epi-LPOGadget-item-nameCompact">
	    <h3><a href="<%=Model.PageLink %>" class="epi-linkBlue"><%=Model.TypeString %></a>
            <% if (Model.IsWinner) { %>
            <span class="epi-LPOGadget-item-winnerIndicator"><%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportWinnerIndicatorText") %></span>               
            <%} %>    
        </h3>
    </div>
    <br />
	<% Html.RenderPartial("LpoGauge", Model); %>
	<br />
	<div class="epi-LPOGadget-item-mainValue <%=Model.CssClass %>">
		<span class="epi-LPOGadget-item-mainValue-major"><%=Model.ConversionRateString %></span>
		<span class="epi-LPOGadget-item-mainValue-minor"><%=Model.ConversionRateRangeString %></span>
	</div>
    <br />
    <button type="button" name="setAsWinnerButton" class="epi-LPOGadget-buttonWinner" title="<%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportSetAsWinnerTooltip") %>"><%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportSetAsWinnerText") %></button>
    <input type="hidden" value="<%=Model.ID %>" />
</div>

