<%@ Page Language="c#" AutoEventWireup="False" CodeBehind="CommerceKpiConfig.aspx.cs"
    Inherits="EPiServer.Marketing.KPI.Commerce.Config.CommerceKpiConfig, EPiServer.Marketing.KPI.Commerce" Title="EPiServer Marketing Commerce Kpi Add-on" %>

<%@ Import Namespace="EPiServer" %>
<%@ Register Assembly="EPiServer.UI" Namespace="EPiServer.UI.WebControls" TagPrefix="EPiServerUI" %>

<asp:content contentplaceholderid="HeaderContentRegion" runat="server">
</asp:content>

<asp:content contentplaceholderid="FullRegion" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" LoadScriptsBeforeUI="true"/>

    <div class="epi-contentContainer epi-padding">          
        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= Translate("/commercekpi/admin/displayname")%>
            </h1>
            <asp:Panel ID="ConnectionNote" runat="server" />
            <asp:ValidationSummary runat="server" CssClass="EP-validationSummary" />
            <asp:Panel ID="MessagePanel" runat="server" Visible="false" />
        </div>
     
        <asp:Panel runat="server" class="epi-formArea">
            <fieldset>
                <legend><%= Translate("/commercekpi/admin/description")%></legend>
                <div class="epi-size15">
                    <asp:Label AssociatedControlID="PreferredMarketList" runat="server"><%# Translate("/commercekpi/admin/financialculturepreference") %></asp:Label>
                    <asp:DropDownList ID="PreferredMarketList" runat="server" MaxLength="255" >

                    </asp:DropDownList>
                </div>
                <div align="right">
                    <EPiServerUI:ToolButton runat="server" SkinID="Save" Text="<%$ Resources: EPiServer, commercekpi.admin.save %>" OnClick="Save_OnClick" CssClass="epi-cmsButton-text epi-cmsButton-tools" />
                    <EPiServerUI:ToolButton runat="server" SkinID="Cancel" Text="<%$ Resources: EPiServer, commercekpi.admin.cancel %>" OnClick="Cancel_OnClick" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                </div>
            </fieldset>
        </asp:Panel>
    </div>
</asp:content>
