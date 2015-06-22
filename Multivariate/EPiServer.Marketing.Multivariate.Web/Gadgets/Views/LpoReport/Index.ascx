<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.ViewData>" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Cmo.Gadgets.Models.LpoReport" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%= Html.AntiForgeryToken() %>
<%= Html.ShellValidationSummary() %>
<% Html.RenderPartial("TestDetails", Model); %>
<% Html.RenderPartial("Toolbar", Model); %>
<%=Html.WarningMessage(Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportNotEnoughDataMessage"), !Model.IsEnoughData)%>
<div class="epi-paddingVertical-small epi-overflowAuto">
    <% foreach (TestPageData pageData in Model.PagesData)
        {
            Html.RenderPartial("TestPageDetails", pageData);
        } 
    %>
</div>
<%=Html.Hidden("TestID", Model.ID)%>
<%=Html.Hidden("InvalidConfiguration", Model.InvalidConfiguration)%>
