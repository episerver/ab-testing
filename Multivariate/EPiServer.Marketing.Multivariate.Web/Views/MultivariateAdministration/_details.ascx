<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>" %>


<% if (Model.testState == 0)
    { %>
<span style="color: red">This test has not been started</span>
<% } %>


<%  if (Model.TestResults != null)
    {
        foreach (var result in Model.TestResults)
        { %>
<table>
    <tr>
        <th colspan="2"><%= result.ItemId %></th>
    </tr>
    <tr>
        <th>Views</th>
        <th>Conversions</th>
    </tr>
    <tr>
        <td><%= result.Views %></td>
        <td><%= result.Conversions %> </td>
    </tr>
</table>
<% } %>
<% } %>  