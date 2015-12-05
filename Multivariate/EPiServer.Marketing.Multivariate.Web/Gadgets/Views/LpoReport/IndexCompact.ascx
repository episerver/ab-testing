<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.ViewData>" %>
<%@ Import Namespace="EPiServer.Cmo.Gadgets.Models.LpoReport" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>

<%= Html.AntiForgeryToken() %>
<% Html.RenderPartial("TestDetails", Model); %>
<% Html.RenderPartial("Toolbar", Model); %>
<%=Html.WarningMessage(Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportNotEnoughDataMessage"), !Model.IsEnoughData)%>
<div class="epi-padding-small epi-overflowHidden">
    <% foreach (TestPageData pageData in Model.PagesData)
        {
            Html.RenderPartial("TestPageDetailsCompact", pageData);
        } 
    %>
</div>
<%=Html.Hidden("TestID", Model.ID)%>
<%=Html.Hidden("InvalidConfiguration", Model.InvalidConfiguration)%>
