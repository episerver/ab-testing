<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>

<asp:Content>
<p>&nbsp;&nbsp;<%= Html.ViewLinkButton(LanguageManager.Instance.Translate("/multivariate/gadget/back"), LanguageManager.Instance.Translate("/multivariate/gadget/back"), "Index/?id=1&",  "", "", null)%></p>
<h1><%= LanguageManager.Instance.Translate("/multivariate/gadget/back")%></h1>
</asp:Content>