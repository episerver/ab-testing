<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Marketing.Multivariate.IMultivariateTest>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Framework.Web.Resources"%>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>


<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title></title>
     <asp:PlaceHolder runat="server">
        <%=Page.ClientResources("ShellCore")%>
        <%=Page.ClientResources("ShellCoreLightTheme")%>
        <%= Html.ScriptResource(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/BrokenLinks/BrokenLinks.js"))%>
        <%= Html.CssLink(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/BrokenLinks/BrokenLinks.css"))%>
        <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "system.css"))%>
        <%= Html.CssLink(EPiServer.Web.PageExtensions.ThemeUtility.GetCssThemeUrl(Page, "ToolButton.css"))%>
        <%= Html.ScriptResource(EPiServer.Shell.Paths.ToClientResource("CMS", "ClientResources/ReportCenter/ReportCenter.js"))%>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUtilBySettings("javascript/episerverscriptmanager.js"))%>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.js")) %>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/dialog.js")) %>
        <%= Html.ScriptResource(EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.aspx")) %>
    </asp:PlaceHolder>
    <link href="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/themes/smoothness/jquery-ui.min.css" rel="stylesheet">
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
	<script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js"></script>
    
    <link rel="stylesheet" type="text/css" href="../../Scripts/datetimepicker/jquery.datetimepicker.css"/>
    <script src="../../Scripts/datetimepicker/jquery.js"></script>
    <script src="../../Scripts/datetimepicker/jquery.datetimepicker.full.js"></script>
    <script>
        $(document).ready(function () {
            // displays date time picker for creating new tests
            $('#datetimepickerstart').datetimepicker({
                format: 'Y-m-d H:i',
                step:30
            });
            $('#datetimepickerstop').datetimepicker({
                format: 'Y-m-d H:i',
                step: 30
            });
        });

        $('#btnCancel').click(function () {
            window.location.href = '@Url.Action("Index")';
        });
    </script>
    
</head>
<body>

    <div class="epi-contentContainer epi-padding">
        </div>

    <div class="epi-contentArea">
        </div>
            
    <div class="epi-formArea">
        <fieldset>
            <%= Html.BeginForm("Create","MultivariateAdministration",FormMethod.Post) %>
            <div class="epi-size15">
                    <Label for="TestTitle"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle") %></Label>
                    <%= Html.TextBoxFor(Model=>Model.Title) %>
                </div>
            
            <div class="epi-size15">
                    <Label for="datetimepickerstart"><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart") %></Label>
                    <input id="datetimepickerstart" name="datetimestart" type="text" style="width:175px" value="<%= LanguageManager.Instance.Translate("/multivariate/settings/startdate") %>" />
            </div>
                <div class="epi-size15">
                    <label for="datetimepickerstop"><%= LanguageManager.Instance.Translate("/multivariate/settings/testend") %></label>
                    <input id="datetimepickerstop" name="datetimeend" type="text" style="width:175px" value="<%= LanguageManager.Instance.Translate("/multivariate/settings/enddate") %>"/>
                </div>
                <div class="epi-size15">
                    <Label for="OriginPage" ><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage") %></Label>
                    <%= Html.TextBoxFor(model=>model.OriginalItemId) %>
                        <button type="button" class="epi-cmsButton-text epi-cmsButton-tools" >PagePicker PH</button>
<%--                    <EPiServerUI:ToolButton ID="btnOriginPagePickerPH" text="PagePicker PH" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>--%>

                    <div id="treedialog" class="ui-helper-hidden" >
                        <div align="right">
<%--                            <button type="button" Text="PagePicker PH" Onclick="return onOkClick" class="epi-cmsButton-text epi-cmsButton-tools" ></button>--%>
                            <%--<EPiServerUI:ToolButton runat="server" Text="OK" OnClientClick="return onOkClick()" CssClass="epi-cmsButton-text epi-cmsButton-tools" />
                            <EPiServerUI:ToolButton runat="server" Text="Cancel" OnClientClick="onCloseClick()" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>--%>
                        </div>
                    </div>
                </div>
                <div class="epi-size15">
                    <Label for="VariantPage"><%= LanguageManager.Instance.Translate("/multivariate/settings/variantpage") %></Label>
                    <%= Html.TextBoxFor(model=>model.VariantItemId) %> 
                        <button type="button" class="epi-cmsButton-text epi-cmsButton-tools" >PagePicker PH</button>
                </div>
                <div class="epi-size15">
                    <Label for="ConversionPage"><%= LanguageManager.Instance.Translate("/multivariate/settings/conversionpage") %></Label>
                    <%= Html.TextBoxFor(model=>model.ConversionItemId) %>
                        <button type="button" class="epi-cmsButton-text epi-cmsButton-tools" >PagePicker PH</button>
                </div>
                <div>
                    <%--<EPiServerUI:ToolButton ID="btnCreate" text="Ok"  runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                    <EPiServerUI:ToolButton ID="btnCancel" Text="Cancel" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>--%>
                </div>
            <div>
                <button type="submit" class="epi-cmsButton-text epi-cmsButton-tools">Ok</button>
                <button type="button" id="btnAdd" class="epi-cmsButton-text epi-cmsButton-tools">Cancel</button>

            </div>
            <% Html.EndForm(); %>
        </fieldset>

        </div>
            
</body>
</html>
