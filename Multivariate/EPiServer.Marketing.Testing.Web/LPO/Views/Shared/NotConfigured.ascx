<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<div class="epi-padding-small epi-gadget-notconfigured">
    <%=Html.GadgetIsNotConfiguredMessage(true)%>
</div>
<%=Html.Hidden("InvalidConfiguration", true)%>