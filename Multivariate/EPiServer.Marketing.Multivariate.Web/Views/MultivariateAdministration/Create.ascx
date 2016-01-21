<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Marketing.Multivariate.Web.Models.MultivariateTestViewModel>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Resources" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
<%@ Import Namespace="EPiServer.Web.WebControls" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Model.Enums" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate.Web.Helpers" %>
<%@ Register TagPrefix="EPiServer" Assembly="EpiServer" Namespace="EPiServer.Web.WebControls" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>
<%@ Register TagPrefix="EPiServerUIDataSource" Namespace="EPiServer.Marketing.Multivariate.Web.Models" Assembly="EPiServer.Marketing.Multivariate.Web" %>

<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title></title>
    <asp:PlaceHolder runat="server">
        <%=Page.ClientResources("ShellCore")%>
        <%=Page.ClientResources("ShellCoreLightTheme")%>
        <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "system.css"))%>
        <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "ToolButton.css"))%>
        <%= Html.ScriptResource(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/ReportCenter/ReportCenter.js"))%>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUtilBySettings("javascript/episerverscriptmanager.js"))%>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.js")) %>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/dialog.js")) %>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.aspx")) %>

        <%  
            // Get the relative path of the app i.e. /{PROTECTED_PATH}/EPiServer.Multivariate/
            UIHelper helper = new UIHelper();
            string appUrl = helper.GetAppRelativePath();
        %>

        <link href="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/themes/smoothness/jquery-ui.min.css" rel="stylesheet">
        <link rel="stylesheet" type="text/css" href="<%=appUrl %>scripts/datetimepicker/jquery.datetimepicker.css" />
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
        <script src="<%=appUrl %>scripts/datetimepicker/jquery.datetimepicker.full.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js"></script>

        <script>
            $(document).ready(function () {
                // displays date time picker for creating new tests
                $('#datetimepickerstart').datetimepicker({
                    format: 'Y-m-d H:i',
                    step: 30
                });
                $('#datetimepickerstop').datetimepicker({
                    format: 'Y-m-d H:i',
                    step: 30
                });
                


            });
            var cancelMessage = "Are you sure you wish to cancel?\nAny unsaved changes will be lost!";

            var cancelClick = function (validate) {
                if (validate)
                    if (confirm(cancelMessage)) {
                        location.href = '<%= Url.Action("Index","MultivariateAdministration") %>';
                    } else {
                        return false;
                    }
               location.href = '<%= Url.Action("Index","MultivariateAdministration") %>';
               
            };

        


        </script>
    </asp:PlaceHolder>
</head>
<body class="Sleek">

    <div class="epi-contentContainer epi-padding">

        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= LanguageManager.Instance.Translate("/multivariate/settings/form/title")%>
                <a href="#" title="Help">
                    <img class="EPEdit-CommandTool" align="absmiddle" border="0" alt="Help" src="/App_Themes/Default/Images/Tools/Help.png" /></a>
            </h1>
        </div>

        <div class="epi-formArea">
            <fieldset>
                <legend><%= LanguageManager.Instance.Translate("/multivariate/settings/form/legendtitle")%>
                </legend>
                <% using (Html.BeginForm("Create", "MultivariateAdministration", FormMethod.Post))
                    { %>
                <% if (Model != null)
                    { %>
                <input type="hidden" name="Id" value="<%= Model.id %>" />
                <% }
                    else
                    { %>
                <input type="hidden" name="Id" value="<%= ViewData["TestGuid"] %>" />
                <% } %>


                <div class="epi-size15">
                    <label for="Title"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle") %></label>

                    <%= (Model == null || Model.testState == TestState.Inactive) ? Html.TextBoxFor(model => model.Title) : Html.TextBoxFor(model => model.Title, new {disabled = "disabled"}) %>
                    <span style="color: red">*&nbsp
                        <%= Html.ValidationMessageFor(model => model.Title) %>
                    </span>
                </div>

                <div class="epi-size15">
                    <label for="datetimepickerstart"><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart") %></label>

                    <%= ((Model == null || Model.testState == TestState.Inactive) ?
                            Html.TextBoxFor(model => model.StartDate, new {id = "datetimepickerstart"}) :
                            Html.TextBoxFor(model => model.StartDate, new {disabled = "disabled", id = "datetimepickerstart"})) %>

                    <span style="color: red">*&nbsp
                        <%= Html.ValidationMessageFor(model => model.StartDate) %>
                    </span>

                </div>
                <div class="epi-size15">
                    <label for="datetimepickerstop"><%= LanguageManager.Instance.Translate("/multivariate/settings/testend") %></label>

                    <%= ((Model == null || Model.testState == TestState.Inactive || Model.testState == TestState.Active) ?
                             Html.TextBoxFor(model => model.EndDate, new {id = "datetimepickerstop"}) :
                             Html.TextBoxFor(model => model.EndDate, new {disabled = "disabled", id = "datetimepickerstop"})) %>

                    <span style="color: red">*&nbsp
                    <%= Html.ValidationMessageFor(model => model.EndDate) %>
                    </span>

                </div>
                <div class="epi-size15">
                    <label for="OriginalItem"><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage") %></label>
                    <% if (Model != null)
                        { %>

                    <input data-val="true" data-val-required="The OriginalItem field is required." id="OriginalItem" name="OriginalItem" type="text" style="display: none" value="<%= Model.OriginalItem %>">
                    <input name="OriginalItemDisplay" type="text" size="30" id="OriginalItemDisplay" disabled="disabled" class="epi-tabView-navigation-item-disabled episize240" style="display: inline;" value="<%= Model.OriginalItemDisplay %>">

                    <% }
                        else
                        { %>
                    <input data-val="true" data-val-required="The OriginalItem field is required." id="OriginalItem" name="OriginalItem" type="text" style="display: none" value="">
                    <input name="OriginalItemDisplay" type="text" size="30" id="OriginalItemDisplay" disabled="disabled" class="epi-tabView-navigation-item-disabled episize240" style="display: inline;" value="">
                    <% } %>




                    <% if (Model == null || Model.testState == TestState.Inactive)
                        { %>
                    <span class="epi-cmsButton">
                        <input name="originalItemBtn" type="button" value="..." class="epismallbutton"
                            onclick="EPi.CreatePageBrowserDialog('/EPiServer/CMS/edit/pagebrowser.aspx',
                                                            document.getElementById('OriginalItem').value,
                                                            'True',
                                                            'False',
                                                            'OriginalItemDisplay',
                                                            'OriginalItem', 'en', null, null, false);"></span>

                    <% } %>
                    <span style="color: red">*&nbsp
                    <%= Html.ValidationMessageFor(model => model.OriginalItem) %></span>
                </div>
                <div class="epi-size15">
                    <label for="VariantItem"><%= LanguageManager.Instance.Translate("/multivariate/settings/variantpage") %></label>


                    <% if (Model != null)
                        { %>

                    <input data-val="true" data-val-required="The VariantItem field is required." id="VariantItem" name="VariantItem" type="text" style="display: none" value="<%= Model.VariantItem %>">
                    <input name="variantItemTextBox" type="text" size="30" id="VariantItemDisplay" disabled="disabled" class="epi-tabView-navigation-item-disabled episize240" style="display: inline;" value="<%= Model.VariantItemDisplay %>">
                    <% }
                        else
                        { %>
                    <input data-val="true" data-val-required="The VariantItem field is required." id="VariantItem" name="VariantItem" type="text" style="display: none" value="">
                    <input name="variantItemTextBox" type="text" size="30" id="VariantItemDisplay" disabled="disabled" class="epi-tabView-navigation-item-disabled episize240" style="display: inline;">
                    <% } %>

                    <% if (Model == null || Model.testState == TestState.Inactive)
                        { %>
                    <span class="epi-cmsButton">
                        <input name="variantItemBtn" type="button" value="..." class="epismallbutton"
                            onclick="EPi.CreatePageBrowserDialog('/EPiServer/CMS/edit/pagebrowser.aspx',
                                                            document.getElementById('VariantItem').value,
                                                            'True',
                                                            'False',
                                                            'VariantItemDisplay',
                                                            'VariantItem', 'en', null, null, false);"></span>
                    <% } %>
                    <span style="color: red">*&nbsp
                    <%= Html.ValidationMessageFor(model => model.VariantItem) %></span>

                </div>
            </fieldset>
            <div class="epi-buttonContainer">
                <% if (Model == null || Model.testState == TestState.Inactive || Model.testState == TestState.Active)
                    { %>
                <span class="epi-cmsButton">
                    <input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Save" type="submit" name="ApplyButton" id="btnSave" value="Save" title="Save" onmouseover="EPi.ToolButton.MouseDownHandler(this)" onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)">
                </span>
                <span class="epi-cmsButton">
                    <input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Cancel" type="button" name="CancelButton" id="btnCancel" value="Cancel" title="Cancel" onmouseover="EPi.ToolButton.MouseDownHandler(this)" onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)" onclick="cancelClick(true);" />
                </span>
                <% }
                    else
                    {%>
                <span class="epi-cmsButtondisabled">
                    <input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Save " disabled="disabled" type="submit" name="ApplyButton" id="btnSave" value="Save" title="Save" onmouseover="EPi.ToolButton.MouseDownHandler(this)" onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)">
                </span>
                <span class="epi-cmsButton">
                    <input class="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Cancel" type="button" name="CancelButton" id="btnCancel" value="Cancel" title="Cancel" onmouseover="EPi.ToolButton.MouseDownHandler(this)" onmouseout="EPi.ToolButton.ResetMouseDownHandler(this)" onclick="cancelClick(false);" />
                </span>

                <% }%>
                

                
            </div>
            <% } %>
        </div>
    </div>
</body>
</html>

