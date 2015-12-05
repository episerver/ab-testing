<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.TestPageData>" %>
<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>

<div class="epi-LPOGadget-barBlock">
    <div class="epi-LPOGadget-bar-annotation">
        <strong class="epi-LPOGadget-bar-annotation-value">
            <%= Model.ConversionRateString %>
            <%=Model.ConversionRateRangeString %></strong> <span class="epi-LPOGadget-bar-annotation-page">
                : <a href="<%=Model.PageLink %>" class="epi-linkBlue">
                    <%= Model.PageName %></a></span>
        <% if (Model.IsWinner)
           { %>
        <span class="epi-LPOGadget-item-winnerIndicator">
            <%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportWinnerIndicatorText") %></span>
        <%} %>
    </div>
    <div class="epi-LPOGadget-bar <%= Model.BarCssClass %>">
        <div class="epi-LPOGadget-bar-value" style="width: <%= Model.ConversionRateString %>%;">
        </div>
        <div class="epi-LPOGadget-bar-range" style="left: <%= Helpers.CalculateRangeBarPosition(Model) %>%;
            width: <%= Helpers.CalculateRangeBarWidth(Model) %>%;">
        </div>
        <div class="epi-LPOGadget-bar-tick" style="left: <%= Model.ConversionRateString %>%;">
        </div>
        <div class="epi-LPOGadget-bar-marker" style="left: <%= Model.ConversionRateString %>%;">
        </div>
    </div>
</div>
