<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>

    <div class="epi-contentContainer epi-padding">
        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= LanguageManager.Instance.Translate("/multivariate/settings/displayname")%>
            </h1>
        </div>
        <div>
            <%= Html.ActionLink("Create New Test","Create",null,null) %>
        </div>
    </div>