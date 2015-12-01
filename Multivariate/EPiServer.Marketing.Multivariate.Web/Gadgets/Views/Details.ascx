<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>

<asp:Content>
<p>&nbsp;&nbsp;<%= Html.ViewLinkButton("Back", "Back", "Index",  "", "", null)%></p>
<h1>Details</h1>
</asp:Content>