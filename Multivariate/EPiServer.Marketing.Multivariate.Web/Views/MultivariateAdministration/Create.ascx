<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Marketing.Multivariate.Web.Models.Entities.MultivariateTestViewModel>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Framework.Web.Resources" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.UI.Admin.MasterPages" %>
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

        <link href="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/themes/smoothness/jquery-ui.min.css" rel="stylesheet">
        <link rel="stylesheet" type="text/css" href="../Scripts/datetimepicker/jquery.datetimepicker.css" />
        <script src="../Scripts/datetimepicker/jquery.js"></script>
        <script src="../Scripts/datetimepicker/jquery.datetimepicker.full.js"></script>
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

                var dlg = $("#treedialog").dialog({
                    autoOpen: false,
                    modal: true
                });

                $("#btnOriginPagePickerPH").click(function () {
                    dlg.dialog("open");
                    $(".ui-dialog-titlebar").hide();

                });

                $("#btnDlgCancel").click(function () {
                    dlg.dialog("close");
                });

                $("#btnDlgOk").click(function () {
                    dlg.dialog("close");
                });

                $('#btnCancel').click(function () {
                    location.href = '<%= Url.Action("Index","MultivariateAdministration") %>';
                });
            });



        </script>


    </asp:PlaceHolder>


</head>
<body>

    <div class="epi-contentContainer epi-padding">
    </div>

    <div class="epi-contentArea">
    </div>

    <div class="epi-formArea">
        <fieldset>
            <% using (Html.BeginForm("Create", "MultivariateAdministration", FormMethod.Post))
                { %>
            <div class="epi-size15">
                <label for="Title"><%= LanguageManager.Instance.Translate("/multivariate/settings/testtitle") %></label>
                <%= Html.TextBoxFor(Model => Model.Title) %>
                <span style="color: red">*&nbsp
                        <%= Html.ValidationMessageFor(Model => Model.Title) %>
                </span>
            </div>

            <div class="epi-size15">
                <label for="datetimepickerstart"><%= LanguageManager.Instance.Translate("/multivariate/settings/teststart") %></label>
                <%= Html.TextBoxFor(Model => Model.StartDate, new {id = "datetimepickerstart"}) %>
                <span style="color: red">*&nbsp
                        <%= Html.ValidationMessageFor(Model => Model.StartDate) %>
                </span>

            </div>
            <div class="epi-size15">
                <label for="datetimepickerstop"><%= LanguageManager.Instance.Translate("/multivariate/settings/testend") %></label>
                <%= Html.TextBoxFor(Model => Model.EndDate, new {id = "datetimepickerstop"}) %>
                <span style="color: red">*&nbsp
                    <%= Html.ValidationMessageFor(Model => Model.EndDate) %>
                </span>
            </div>
            <div class="epi-size15">
                <label for="OriginPage"><%= LanguageManager.Instance.Translate("/multivariate/settings/originpage") %></label>
                <%= Html.TextBoxFor(model => model.OriginalItemId) %>
                <span class="epi-cmsButton">
                    <input id="btnOriginPagePickerPH" type="button" value="..." /></span>
                <span style="color: red">*</span>
                <%--                    <EPiServerUI:ToolButton ID="btnOriginPagePickerPH" text="PagePicker PH" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>--%>

                <div id="treedialog" class="ui-helper-hidden">
                    <EPiServerUIDataSource:PageDataSource ID="contentDataSource"
                        AccessLevel="NoAccess"
                        runat="server"
                        IncludeRootItem="false"
                        ContentLink="<%# EPiServer.Web.SiteDefinition.Current.RootPage %>" />
                    <div class="episcroll episerver-pagetree-selfcontained" style="max-height: 250px;">
                        <EPiServerUI:PageTreeView ID="pageTreeView"
                            DataSourceID="contentDataSource"
                            CssClass="episerver-pagetreeview"
                            runat="server"
                            ExpandDepth="1"
                            DataTextField="Name"
                            ExpandOnSelect="false"
                            DataNavigateUrlField="ContentLink" EnableViewState="false">
                            <treenodetemplate>
                                    <a style="outline:none !important" href="#" title="<%# Server.HtmlEncode(((PageTreeNode)Container.DataItem).Text) %>" onclick="return setval(<%# ((EPiServer.Core.IContent)((PageTreeNode)Container.DataItem).DataItem).ContentLink.ID %>)" >
                                        <%# Server.HtmlEncode(((PageTreeNode)Container.DataItem).Text) %>
                                    </a>
                                </treenodetemplate>
                        </EPiServerUI:PageTreeView>
                    </div>

                    <div align="right">
                        <span class="epi-cmsButton">
                            <input id="btnDlgOk" type="button" value="OK" /></span>
                        <span class="epi-cmsButton">
                            <input id="btnDlgCancel" type="button" value="Cancel" /></span>
                    </div>
                </div>
            </div>
            <%-- <div class="epi-size15">
                <label for="VariantPage"><%= LanguageManager.Instance.Translate("/multivariate/settings/variantpage") %></label>
                <%= Html.TextBoxFor(model => model.VariantItemId) %>
                <button type="button" class="epi-cmsButton-text epi-cmsButton-tools">PagePicker PH</button>
                <span style="color: red">*</span>
            </div>
            <div class="epi-size15">
                <label for="ConversionPage"><%= LanguageManager.Instance.Translate("/multivariate/settings/conversionpage") %></label>
                <%= Html.TextBoxFor(model => model.ConversionItemId) %>
                <button type="button" class="epi-cmsButton-text epi-cmsButton-tools">PagePicker PH</button>
                <span style="color: red">*</span>
            </div>--%>
            <div>
                <%--<EPiServerUI:ToolButton ID="btnCreate" text="Ok"  runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                    <EPiServerUI:ToolButton ID="btnCancel" Text="Cancel" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>--%>
            </div>
            <div>
                <button type="submit" class="epi-cmsButton-text epi-cmsButton-tools">Ok</button>
                <button type="button" id="btnCancel" class="epi-cmsButton-text epi-cmsButton-tools">Cancel</button>

            </div>
            <% } %>
        </fieldset>

    </div>

</body>
</html>
