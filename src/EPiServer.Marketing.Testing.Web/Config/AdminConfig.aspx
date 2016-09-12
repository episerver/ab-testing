<%@ Page Language="c#" AutoEventWireup="False" CodeBehind="AdminConfig.aspx.cs"
    Inherits="EPiServer.Marketing.Testing.Web.Config.AdminConfig, EPiServer.Marketing.Testing.Web" Title="EPiServer Marketing Testing Add-on" %>

<%@ Import Namespace="EPiServer" %>
<%@ Register Assembly="EPiServer.UI" Namespace="EPiServer.UI.WebControls" TagPrefix="EPiServerUI" %>

<asp:content contentplaceholderid="HeaderContentRegion" runat="server">
</asp:content>

<asp:content contentplaceholderid="FullRegion" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" LoadScriptsBeforeUI="true"/>

    <div class="epi-contentContainer epi-padding">               
        <asp:Panel runat="server" class="epi-formArea">
            <fieldset>
                <legend><%= Translate("/abtesting/admin/displayname")%></legend>
                <div class="epi-size15">
                    <asp:Label AssociatedControlID="TestDuration" runat="server"><%# Translate("/abtesting/admin/testduration") %></asp:Label>
                    <asp:TextBox ID="TestDuration" runat="server" MaxLength="255" Text="<%# TestSettings.TestDuration %>" />
                    <asp:RangeValidator ID="DurationValidator" ErrorMessage='<%# Translate("/abtesting/admin/durationerror") %>' runat="server" ControlToValidate="TestDuration" MaximumValue="365" MinimumValue="1" Type="Integer" />
                </div>
                <div class="epi-size15">
                    <asp:Label AssociatedControlID="ParticipationPercent" runat="server"><%# Translate("/abtesting/admin/participationpercent") %></asp:Label>
                    <asp:TextBox ID="ParticipationPercent" runat="server" MaxLength="255" Text="<%# TestSettings.ParticipationPercent %>" />
                    <asp:RangeValidator ID="ParticipationValidator" ErrorMessage='<%# Translate("/abtesting/admin/participationerror") %>' runat="server" ControlToValidate="ParticipationPercent" MaximumValue="100" MinimumValue="1" Type="Integer" />
                </div>
                <div class="epi-size15">
                    <asp:Label AssociatedControlID="ConfidenceLevel" runat="server"><%# Translate("/abtesting/admin/confidencelevel") %></asp:Label>
                    <asp:DropDownList ID="ConfidenceLevel" runat="server" MaxLength="255" Text="<%# TestSettings.ConfidenceLevel %>" >
                        <asp:ListItem Value="99"> 99% </asp:ListItem>
                        <asp:ListItem Value="98"> 98% </asp:ListItem>
                        <asp:ListItem Value="95"> 95% </asp:ListItem>
                        <asp:ListItem Value="90"> 90% </asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div align="right">
                    <EPiServerUI:ToolButton runat="server" Text="<%$ Resources: EPiServer, abtesting.admin.save %>" OnClick="Save_OnClick" CssClass="epi-cmsButton-text epi-cmsButton-tools" />
                    <EPiServerUI:ToolButton runat="server" Text="<%$ Resources: EPiServer, abtesting.admin.cancel %>" OnClick="Cancel_OnClick" CssClass="epi-cmsButton-text epi-cmsButton-tools"/>
                </div>
            </fieldset>
        </asp:Panel>
    </div>
</asp:content>
