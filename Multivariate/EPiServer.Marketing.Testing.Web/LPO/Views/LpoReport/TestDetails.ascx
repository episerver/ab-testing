<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.ViewData>" %>

<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<div class="epi-padding-small epi-overflowHidden epi-gadget-header">
    <h1><%: Model.Name %> (<%=Model.State %><% if (Model.IsBroken) { %>,&nbsp;<span class="epi-color-red"><%= Html.Translate("/EPiServer/Cmo/Gadgets/StateLPOBRK") %></span><% } %>) </h1>
    <div>
        <span class="epi-CMOGadget-headingMeta">
            <span class="epi-CMOGadget-headingMeta-Name"><%=Html.Translate("/EPiServer/Cmo/Gadgets/LanguageTitle")%></span>
            <%=Html.LanguageLabel(Model.Language) %>
        </span>
        <span class="epi-CMOGadget-headingMeta">
            <span class="epi-CMOGadget-headingMeta-Name"><%=Html.Translate("/EPiServer/Cmo/Gadgets/LpoTestPeriodTitle")%></span><span class="epi-CMOGadget-headingMeta-Value"><%=Model.Period %></span>
        </span>
        <span class="epi-CMOGadget-headingMeta"><span class="epi-CMOGadget-headingMeta-Name"><%=Html.Translate("/EPiServer/Cmo/Gadgets/ConversionPageTitle")%></span>
            <span class="epi-CMOGadget-headingMeta-Value">
                <%=Html.LinkOrSpan(String.Empty, Model.ConversionPageName, Model.ConversionPageExistsInCMS ? Model.ConversionPageLink : String.Empty, "epi-linkBlue", "epi-LPOGadget-deleted")%>
            </span>
        </span>
    </div>
</div>