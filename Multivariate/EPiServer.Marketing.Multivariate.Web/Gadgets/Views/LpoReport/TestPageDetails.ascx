<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.TestPageData>" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<div class="epi-LPOGadget-item<%= Model.IsWinner ? " epi-LPOGadget-item-winner" : String.Empty %>">
	<div class="epi-LPOGadget-item-name">
		<strong><%=Model.TypeString %>:</strong>
        <%=Html.LinkOrSpan(String.Empty, Model.PageName, Model.PageExistsInCMS ? Model.PageLink : String.Empty, "epi-linkBlue", "epi-LPOGadget-deleted")%>
        <% if (Model.IsWinner) { %>
        <span class="epi-LPOGadget-item-winnerIndicator"><%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportWinnerIndicatorText") %></span>               
        <%} %>
        
	</div>
	<div class="epi-clear">
		<div class="epi-LPOGadget-item-thumb">
			<img style="width:164px;height:121px;" src="<%=Model.PageThumbnailUrl %>" alt="<%=Model.TypeString %> Snapshot" />
		</div>
		<div class="epi-LPOGadget-item-stats">
			<div class="epi-LPOGadget-item-mainValue <%=Model.CssClass %>">
				<span class="epi-LPOGadget-item-mainValue-major"><%=Model.ConversionRateString %></span>
				<span class="epi-LPOGadget-item-mainValue-minor"><%=Model.ConversionRateRangeString %></span>
			</div>
			<span class="epi-LPOGadget-item-value">
				<%=Html.Translate("/EPiServer/Cmo/Gadgets/ChanceToBeatOriginalTitle")%>
				<span><%=Model.ChanceToBeatOriginalString %></span>
			</span>
			<span class="epi-LPOGadget-item-value">
				<%=Html.Translate("/EPiServer/Cmo/Gadgets/ChanceToBeatAllTitle") %>
				<span><%=Model.ChanceToBeatAllString %></span>
			</span>
			<span class="epi-LPOGadget-item-value">
				<%=Html.Translate("/EPiServer/Cmo/Gadgets/ObservedImprovementTitle") %>
				<span><%=Model.ObservedImprovementString %></span>
			</span>
			<span class="epi-LPOGadget-item-value">
				<%=Html.Translate("/EPiServer/Cmo/Gadgets/ConversionsImpressionsTitle") %>
				<span><%=Model.Conversions %> / <%=Model.Impressions %></span>
			</span>
            <button type="button" name="setAsWinnerButton" class="epi-LPOGadget-buttonWinner" title="<%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportSetAsWinnerTooltip") %>"><%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportSetAsWinnerText") %></button>
            <input type="hidden" value="<%=Model.ID %>" />
		</div>
        <% Html.RenderPartial("LpoGauge", Model); %>
	</div>
</div>    


