<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.ViewData>" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<div class="epi-gadget-toolbar">
    <div class="epi-gadget-toolbar-container">
        <%= Html.ToolButton("startButton", Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportStartButtonText"), Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportStartButtonTooltip"), "epi-gadget-toolbar-button", "epi-gadget-toolbar-button-disabled", "epi-gadget-toolbar-button-Start", 
            Model.CanBeStarted, Model.CanBeStarted || (!Model.CanBeStopped && !Model.CanBeFinalized), null) %>
        <%= Html.ToolButton("stopButton", Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportStopButtonText"), Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportStopButtonTooltip"), "epi-gadget-toolbar-button", "epi-gadget-toolbar-button-disabled", "epi-gadget-toolbar-button-Stop", 
            Model.CanBeStopped, Model.CanBeStopped, null)%>
        <%= Html.ToolButton("finalizeButton", Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportFinalizeButtonText"), Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportFinalizeButtonTooltip"), "epi-gadget-toolbar-button", "epi-gadget-toolbar-button-disabled", "epi-gadget-toolbar-button-Archive", 
            Model.CanBeFinalized, Model.CanBeFinalized, null)%>
    </div>
</div>
