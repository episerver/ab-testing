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
    </div>

    <div id="TestSettingsForm" runat="server" class="epi-formArea">
        <fieldset>
            <legend> <%= Translate("/multivariate/settings/description") %></legend>

            <div class="epi-size15">
                <asp:label AssociateControlID="TestTitle" runat="server"><%= Translate("/multivariate/settings/testtitle") %></asp:label>
                <asp:TextBox ID="TestTitle" runat="server" MaxLength="255" />
            </div>
            <div class="epi-size15">
                <asp:label AssociateControlID="TestStart" runat="server"><%= Translate("/multivariate/settings/teststart") %></asp:label>
                <asp:label ID="TestStart" runat="server" MaxLength="255" >[DatePicker PH]</asp:label>
            </div>
            <div class="epi-size15">
                <asp:label AssociateControlID="TestStop" runat="server"><%= Translate("/multivariate/settings/testend") %></asp:label>
                <asp:label ID="TestStop" runat="server" MaxLength="255" >[DatePicker PH]</asp:label>
            </div>
            
            <div class="epi-size15">
                <asp:label AssociateControlID="OriginPage" runat="server"><%= Translate("/multivariate/settings/originpage") %></asp:label>
                <asp:label ID="OriginPage" runat="server" MaxLength="255" >[PagePicker PH]</asp:label>
            </div>
            
            <div class="epi-size15">
                <asp:label AssociateControlID="VariantPage" runat="server"><%= Translate("/multivariate/settings/variantpage") %></asp:label>
                <asp:label ID="VariantPage" runat="server" MaxLength="255" >[PagePicker PH]</asp:label>
            </div>
            
             <div class="epi-size15">
                <asp:label AssociateControlID="ConversionPage" runat="server"><%= Translate("/multivariate/settings/conversionpage") %></asp:label>
                <asp:label ID="ConversionPage" runat="server" MaxLength="255" >[PagePicker PH]</asp:label>
            </div>

            <div>
                <asp:button ID="CreateTest" runat="server" text="Submit" OnClick="Create_Test"/>
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
    


    
</asp:content>