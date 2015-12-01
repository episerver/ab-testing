<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IList<IMultivariateTest>>" %>
<%@ Import Namespace="EPiServer.Marketing.Multivariate" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html"%>

<asp:Content>
<p>&nbsp;&nbsp;<%= Html.ViewLinkButton("Refresh", "Refresh", "Index/?id=1&",  "", "", null)%></p>
	<table class="epi-default">
	<tr>
		<th class="episize300">Name</th>
		<th class="episize300">Start</th>
		<th class="episize300">Finish</th>
		<th class="episize300">State</th>
		<th class="episize300">Original</th>
		<th class="episize300">Winning Variation</th>
		<th class="episize300">Conversion</th>
		<th class="episize300">Actions</th>
	</tr>
	
	<% foreach (var item in Model) { %>
	<tr>
        <td><%= item.Title%></td>
        <td><%= item.StartDate%></td>
        <td><%= item.EndDate%></td>
        <td><%= item.State%></td>
        <td><%= item.OriginalItemId%></td>
        <td><%= item.ConversionItemId%></td>
        <td>58%</td>
	<td>
            <a length="0" class="epi-button-child-item" href="#" onclick="epi.gadget.loadView(this, {'action':'Details/?id=<%=item.Id%>&'});return false;"
		title="Details">
                <img border="0" alt="Details" src="document_selected.gif">
		</a>
            <a href="" title="Edit">
                <img border="0" alt="Edit" src="table_selected.gif">
            </a>
            <a href="" title="Promote">
                <img border="0" alt="Promote" src="maximize_over.gif">
            </a>
	</td>                    
	</tr>
	<% } %>
	<tr bgcolor="#d3d3d3">
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	<td>&nbsp;</td>
	</tr>
    <tr>
                <td>Call to Action</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Active</td>
                <td>CallToAction</td>
                <td>CallToActionWithPromotion</td>
                <td>58%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
	<tr>
                <td>Generate Leads</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Inactive</td>
                <td>GenLeads</td>
                <td>GenLeadsWithSkiPromotion</td>
                <td>74%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
	<tr>
                <td>Buy more consulting</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Done</td>
                <td>RequestConsulting</td>
                <td>RequestConsultingWithDiscounts</td>
                <td>14%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
	<tr>
                <td>Buy Add Ons (15% discount for 5)</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Active</td>
                <td>Buy Add Ons</td>
                <td>Some winner</td>
                <td>37%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
	<tr>
                <td>Buy Add Ons (10% discount for 4)</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Archived</td>
                <td>BuyMoreDiscount2</td>
                <td>BuyMoreDiscount4</td>
                <td>10%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
	<tr>
                <td>Buy Add Ons (5% discount for 2)</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Archived</td>
                <td>Buy Add Ons</td> 
                <td>BuyMoreDiscount2</td>
                <td>17%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
	<tr>
                <td>Buy Add Ons (5% discount)</td>
                <td>2015-11-23 13:32:10</td>
                <td>2015-11-23 13:32:10</td>
                <td>Archived</td>
                <td>Buy Add Ons</td>
                <td>BuyMoreDiscount</td>
                <td>16%</td>
		<td>
                    <a href="" title="Details">
                        <img border="0" alt="Details" src="document_selected.gif">
                    </a>
                    <a href="" title="Edit">
                        <img border="0" alt="Edit" src="table_selected.gif">
                    </a>
                    <a href="" title="Promote">
                        <img border="0" alt="Promote" src="maximize_over.gif">
                    </a>
		</td>                    
	</tr>
    </table>
</asp:Content>