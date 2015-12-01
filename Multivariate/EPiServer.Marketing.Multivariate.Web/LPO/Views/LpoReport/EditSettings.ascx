<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.Settings>" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<div class="epi-paddingHorizontal-small epi-formArea">
    
    <%= Html.ShellValidationSummary() %>
    <% Html.BeginGadgetForm("EditSettings"); %>    
    <%= Html.AntiForgeryToken() %>
    <fieldset>
        <legend><%= Html.Translate("/EPiServer/Cmo/Gadgets/SettingsTitle")%></legend>
        <div class="epi-size15">
            <div>
                <label><%= Html.Translate("/EPiServer/Cmo/Gadgets/SelectLpoTestLabel")%></label><br />
                <%= Html.DropDownList("LpoTestID", Helpers.CreateSelectList(Model.LpoTests, Model.LpoTestID),
                    new { @class = "epi-width100", @size = "10" })%>
            </div>
        </div>
        <div>
            <%= Html.LabeledCheckBox("IsCompactView", Html.Translate("/EPiServer/Cmo/Gadgets/LpoReportCompactView"), Model.IsCompactView)%>
        </div>
    </fieldset>
    <div class="epi-buttonContainer-simple">
        <% =Html.AcceptButton(new { @class="epi-button-child-item"}) %>
        <% =Html.CancelButton(new { @class="epi-button-child-item"}) %>
    </div>
    <% Html.EndForm(); %>
</div>

