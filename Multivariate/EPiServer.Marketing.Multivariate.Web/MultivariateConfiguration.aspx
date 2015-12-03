<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="MultivariateConfiguration.aspx.cs"
    Inherits="EPiServer.Marketing.Multivariate.Web.MultivariateConfiguration, EPiServer.Marketing.Multivariate.Web" %>

<%@ Import Namespace="EPiServer" %>
<%@ Register Assembly="EPiServer.UI" Namespace="EPiServer.UI.WebControls" TagPrefix="EPiServerUI" %>
<%@ Register TagPrefix="EPiServerUIDataSource" Namespace="EPiServer.Marketing.Multivariate.Web.Models" Assembly="EPiServer.Marketing.Multivariate.Web" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.Edit" Assembly="EPiServer.UI, Version=9.3.1.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7" %>

<asp:content contentplaceholderid="HeaderContentRegion" runat="server">
</asp:content>

<asp:content contentplaceholderid="FullRegion" runat="server">
    <script src="Scripts/datetimepicker/jquery.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
	<script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js"></script>
    <script src="Scripts/datetimepicker/jquery.datetimepicker.full.js"></script>
    <link rel="stylesheet" type="text/css" href="Scripts/datetimepicker/jquery.datetimepicker.css"/>
    <link href="//ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/themes/smoothness/jquery-ui.min.css" rel="stylesheet">

    <script type="text/javascript">
        $(document).ready(function () {
            var dlg = $("#treedialog").dialog({
                autoOpen: false,
                modal: true
            });

            $("#btnOriginPagePickerPH").click(function () {
                dlg.dialog("open");
                $(".ui-dialog-titlebar").hide();

            });

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

        // called when user clicks Ok in select folder dlg
        function onOkClick() {
            $("#treedialog").dialog("close");
        }

        // called when user clicks Cancel in select folder dlg
        function onCloseClick() {
            $("#treedialog").dialog("close");
        }

    </script>
    
    <div class="epi-contentContainer epi-padding">
        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= Translate("/multivariate/settings/displayname")%>
            </h1>
        </div>
    


        <asp:Panel class="epi-formArea" runat="server">
            <fieldset>
                <legend> <%= Translate("/multivariate/settings/description") %></legend>

                <div class="epi-size15">
                    <asp:label AssociatedControlID="TestTitle" runat="server"><%= Translate("/multivariate/settings/testtitle") %></asp:label>
                    <asp:TextBox ID="TestTitle" MaxLength="255" runat="server"/>
                </div>
                <div class="epi-size15">
                    <label for="datetimepickerstart" runat="server"><%= Translate("/multivariate/settings/teststart") %></label>
                    <input id="datetimepickerstart" name="datetimestart" type="text" style="width:175px" value="<%= Translate("/multivariate/settings/startdate") %>" />
                </div>
                <div class="epi-size15">
                    <label for="datetimepickerstop" runat="server"><%= Translate("/multivariate/settings/testend") %></label>
                    <input id="datetimepickerstop" name="datetimeend" type="text" style="width:175px" value="<%= Translate("/multivariate/settings/enddate") %>"/>
                </div>
                <div class="epi-size15">
                    <asp:label AssociatedControlID="OriginPage" runat="server"><%= Translate("/multivariate/settings/originpage") %></asp:label>
                    <asp:TextBox ID="OriginPage" MaxLength="255" runat="server" Text="1"/>
                    <span class="epi-cmsButton"><input id="btnOriginPagePickerPH" type="button" value="..."/></span>
                    <div id="treedialog" class="ui-helper-hidden" >
                        <EPiServerUIDataSource:PageDataSource ID="contentDataSource" 
                            AccessLevel="NoAccess" 
                            runat="server" 
                            IncludeRootItem="false" 
                            ContentLink="<%# EPiServer.Web.SiteDefinition.Current.RootPage %>" />
                       <div class="episcroll episerver-pagetree-selfcontained" style="max-height:250px;">
                           <EPiServerUI:PageTreeView ID="pageTreeView" 
                               DataSourceID="contentDataSource" 
                               CssClass="episerver-pagetreeview" 
                               runat="server" 
                               ExpandDepth="1"
                               DataTextField="Name" 
                               ExpandOnSelect="false"
                               DataNavigateUrlField="ContentLink" EnableViewState="false">
                                <TreeNodeTemplate>
                                    <a style="outline:none !important" href="#" title="<%# Server.HtmlEncode(((PageTreeNode)Container.DataItem).Text) %>" onclick="return setval(<%# ((EPiServer.Core.IContent)((PageTreeNode)Container.DataItem).DataItem).ContentLink.ID %>)" >
                                        <%# Server.HtmlEncode(((PageTreeNode)Container.DataItem).Text) %>
                                    </a>
                                </TreeNodeTemplate>
                            </EPiServerUI:PageTreeView>
                        </div> 
                        
                        <div align="right">
                            <EPiServerUI:ToolButton runat="server" Text="OK" OnClientClick="return onOkClick()" CssClass="epi-cmsButton-text epi-cmsButton-tools" />
                            <EPiServerUI:ToolButton runat="server" Text="Cancel" OnClientClick="onCloseClick()" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                        </div>
                    </div>
                </div>
                <div class="epi-size15">
                    <asp:label AssociatedControlID="VariantPage" runat="server"><%= Translate("/multivariate/settings/variantpage") %></asp:label>
                    <asp:TextBox ID="VariantPage" MaxLength="255" runat="server" Text="2"/>
                    <EPiServerUI:ToolButton ID="btnVariantPagePickerPH" Text="PagePicker PH" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                </div>
                <div class="epi-size15">
                    <asp:label AssociatedControlID="ConversionPage" runat="server"><%= Translate("/multivariate/settings/conversionpage") %></asp:label>
                    <asp:TextBox ID="ConversionPage"  MaxLength="255" runat="server" Text="3"></asp:TextBox>
                    <EPiServerUI:ToolButton ID="btnConversionPagePickerPH" Text="PagePicker PH" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                </div>
                <div>
                    <EPiServerUI:ToolButton ID="btnCreate" text="Ok" OnClick="Create_Test" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                    <EPiServerUI:ToolButton Tasks ID="btnCancel" Text="Cancel" OnClick="Cancel_Create" runat="server" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                </div>
                 
            </fieldset>
        </asp:panel>
        
        <div class="epi-contentContainer epi-padding">
        <div class="epi-contentArea">
        <div>
            <asp:GridView runat="server" ID="Grid" cssClass="epi-padding">
                
            </asp:GridView>
        </div>
            </div>
            </div>
        
                
            
    </div>
 </asp:content>
