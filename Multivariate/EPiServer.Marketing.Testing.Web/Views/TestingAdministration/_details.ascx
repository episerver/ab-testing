<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Marketing.Testing.Web.Models.ABTestViewModel>" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Editor" %>
<%@ Import Namespace="EPiServer.Marketing.Testing.Web.Helpers" %>
<%@ Import Namespace="EPiServer.Web.Mvc.Html" %>


<!-- Message indicator: Shows when a test has not been started -->
<div>
    <% if (Model.testState == 0)
        { %>
    <span style="color: red">This test has not been started</span>
    <% } %>
</div>


<!-- Test Results Section -->
<div class="epi-contentContainer epi-padding">

    <div class="epi-contentArea" style="float: right">

        <table>
            <caption class="detailsPadding" style="text-align: center; font-weight: bold">
                <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/testresults/tablecaption")%>
            </caption>
            <tr class="detailsPadding">
                <th>Content</th>
                <th style="padding-right: 15px">Link</th>
                <th style="padding-right: 15px">Type</th>
                <th style="padding-right: 15px">Views</th>
                <th style="padding-right: 15px">Convs</th>
                <th style="padding-right: 15px">Conv %</th>
            </tr>

            <!-- Generate Test Result Rows -->
            <%  UIHelper helper = new UIHelper();

                if (Model.TestResults != null)
                {
                    foreach (var result in Model.TestResults)
                    { %>
            <tr class="detailsPadding">
                <td style="padding-right: 15px"><%= helper.getContent(result.ItemId).Name %></td>
                <td><%= helper.getContent(result.ItemId).ContentLink %></td>
                <td><%= helper.getContent(result.ItemId).ContentTypeID  %></td>
                <td><%= result.Views %></td>
                <td><%= result.Conversions %> </td>
                <td></td>
            </tr>

            <% } %>
            <% } %>
        </table>
    </div>

    <!-- Test Information Section -->
    <fieldset>
        <div class="epi-contentArea">
            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/title")%>
                </div>
                <div class="display-field"><%= Html.Encode(Model.Title) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/owner")%>
                </div>
                <div class="display-field"><%= Html.Encode(Model.Owner)%> </div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/created")%>
                </div>
                <div class="display-field"><%= Html.Encode(Model.DateCreated.ToLocalTime()) %> by <%= Html.Encode(Model.Owner) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/modified")%>
                </div>
                <% if (Model.DateModified == Model.DateCreated)
                    { %>
                <div class="display-field" style="font-style: oblique">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/unmodified")%>
                </div>
                <% }
                    else
                    { %>
                <div class="display-field"><%= Html.Encode(Model.DateModified.ToLocalTime()) %></div>
                <% }%>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/teststate")%>
                </div>
                <div class="display-field"><%= Html.Encode(Model.testState) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/start")%>
                </div>
                <div class="display-field"><%= Html.Encode(Model.StartDate) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">
                    <%= LanguageManager.Instance.Translate("/multivariate/settings/configurationview/testdetails/end")%>
                </div>
                <div class="display-field"><%= Html.Encode(Model.EndDate) %></div>
            </div>

        </div>
    </fieldset>

</div>
