<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.TestPageData>" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<div class="epi-LPOGadget-item<%= Model.IsWinner ? " epi-LPOGadget-item-winner" : String.Empty %>">
    <% Html.RenderPartial("TestPageBar", Model); %>
	<div class="epi-clear">
		<div class="epi-LPOGadget-item-stats">
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
	</div>
</div>    


