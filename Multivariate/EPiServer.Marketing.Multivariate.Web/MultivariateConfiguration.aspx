<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="MultivariateConfiguration.aspx.cs"
    Inherits="EPiServer.Marketing.Multivariate.Web.MultivariateConfiguration" %>

<%@ Import Namespace="EPiServer" %>
<%@ Register Assembly="EPiServer.UI" Namespace="EPiServer.UI.WebControls" TagPrefix="EPiServerUI" %>
<%@ Register TagPrefix="EPiServerUIDataSource" Namespace="EPiServer.Marketing.Multivariate.Web.Models" Assembly="EPiServer.Marketing.Multivariate.Web" %>

<asp:content contentplaceholderid="HeaderContentRegion" runat="server">
</asp:content>

<asp:content contentplaceholderid="FullRegion" runat="server">
    <div class="epi-contentContainer epi-padding">
        <div class="epi-contentArea">
            <h1 class="EP-prefix">
                <%= Translate("/multivariate/settings/displayname")%>
            </h1>
        </div>
    

        <div id="TestSettingsForm" runat="server" class="epi-formArea">
            <fieldset>
                <legend> <%= Translate("/multivariate/settings/description") %></legend>

                <div class="epi-size15">
                    <asp:label AssociateControlID="TestTitle" runat="server"><%= Translate("/multivariate/settings/testtitle") %></asp:label>
                    <asp:TextBox ID="TestTitle" MaxLength="255" runat="server"/>
                </div>
                <div class="epi-size15">
                    <asp:label AssociateControlID="TestStart" runat="server"><%= Translate("/multivariate/settings/teststart") %></asp:label>
                    <asp:label ID="TestStart" MaxLength="255" runat="server">[DatePicker PH]</asp:label>
                </div>
                <div class="epi-size15">
                    <asp:label AssociateControlID="TestStop" runat="server"><%= Translate("/multivariate/settings/testend") %></asp:label>
                    <asp:label ID="TestStop" MaxLength="255" runat="server" >[DatePicker PH]</asp:label>
                </div>
            
                <div class="epi-size15">
                    <asp:label AssociateControlID="OriginPage" runat="server"><%= Translate("/multivariate/settings/originpage") %></asp:label>
                    <asp:TextBox ID="OriginPage" MaxLength="255" runat="server"/>
                    <asp:Button ID="btnOriginPagePickerPH" Text="PagePicker PH" runat="server"/>
                </div>
                
                <div class="epi-size15">
                    <asp:label AssociateControlID="VariantPage" runat="server"><%= Translate("/multivariate/settings/variantpage") %></asp:label>
                    <asp:TextBox ID="VariantPage" MaxLength="255" runat="server"/>
                    <asp:Button ID="btnVariantPagePickerPH" Text="PagePicker PH" runat="server"/>
                </div>
            
                 <div class="epi-size15">
                    <asp:label AssociateControlID="ConversionPage" runat="server"><%= Translate("/multivariate/settings/conversionpage") %></asp:label>
                    <asp:TextBox ID="ConversionPage"  MaxLength="255" runat="server"></asp:TextBox>
                    <asp:Button ID="btnConversionPagePickerPH" Text="PagePicker PH" runat="server"/>
                </div>

                <div>
                    <asp:button ID="btnCreate" text="Submit" OnClick="Create_Test" runat="server"/>
                    <asp:Button ID="btnCancel" Text="Cancel" OnClick="Cancel_Create" runat="server"/>
                </div>

            </fieldset>
        </div>

        <div id="TestListReport" class="epi-contentContainer epi-padding">
            <div class="epi-contentArea">
                <asp:GridView ID="TestReport" runat="server" BackColor="LightGoldenrodYellow" BorderColor="Tan"
                    Caption="Current Multivariate Test Reports" AllowPaging="true" AllowSorting="False"
                    BorderWidth="1px" CellPadding="2" ForeColor="Black" GridLines="None">
                </asp:GridView> 
            </div>
        </div>
    </div>


    
</asp:content>
