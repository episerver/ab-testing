<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Editor" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
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
            <caption class="detailsPadding" style="text-align: center; font-weight: bold">Current Multivariate Test Results</caption>
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
                <div class="display-label">Title:</div>
                <div class="display-field"><%= Html.Encode(Model.Title) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">Current Owner:</div>
                <div class="display-field"><%= Html.Encode(Model.Owner)%> </div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">Created:</div>
                <div class="display-field"><%= Html.Encode(Model.DateCreated.ToLocalTime()) %> by <%= Html.Encode(Model.Owner) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">Last Modified:</div>
                <% if (Model.DateModified == Model.DateCreated)
                    { %>
                <div class="display-field" style="font-style: oblique">Unmodified</div>
                <% }
                    else
                    { %>
                <div class="display-field"><%= Html.Encode(Model.DateModified.ToLocalTime()) %></div>
                <% }%>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">Status:</div>
                <div class="display-field"><%= Html.Encode(Model.testState) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">Start Date:</div>
                <div class="display-field"><%= Html.Encode(Model.StartDate) %></div>
            </div>

            <div class="epi-size15 detailsPadding">
                <div class="display-label">Stop Date:</div>
                <div class="display-field"><%= Html.Encode(Model.EndDate) %></div>
            </div>

        </div>
    </fieldset>

</div>
