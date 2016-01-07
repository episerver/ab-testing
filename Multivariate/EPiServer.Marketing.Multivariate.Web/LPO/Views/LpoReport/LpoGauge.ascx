<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EPiServer.Cmo.Gadgets.Models.LpoReport.TestPageData>" %>
<%@ Import Namespace="EPiServer.Cmo.Gadgets.Util" %>
<div class="epi-LPOGadget-item-gauge"> 
    <object name="gaugeObject" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
        <param name="source" value='<%=EPiServer.Cmo.Cms.Helpers.UrlHelper.ResolveUrlInCmoFromSettings("ClientBin/EPiServer.Cmo.Gauge.xap?ver=7.0") %>' />
        <param name="onError" value="onSilverlightError" />
        <param name="background" value="Transparent" />
        <param name="windowless" value="true" />
        <param name="minRuntimeVersion" value="3.0.40624.0" />
        <param name="autoUpgrade" value="true" />
        <param name="initParams" value="<%=String.Format("GaugeType=LpoGauge,IsOriginal={0},RateValue={1},RateValueConfidenceInterval={2},BaseRateValue={3},BaseRateValueConfidenceInterval={4},CompareResults={5},ToolTip={6}", 
            Model.IsOriginal, Model.ConversionRate, Model.ConversionRateRange, Model.OriginalPageConversionRate, Model.OriginalPageConversionRateRange, Model.ComparePageResult, Model.ConversionRateWithRangeString) %>" />
        <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=3.0.40624.0" style="text-decoration: none">
            <img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight"
                style="border-style: none" />
        </a>
    </object>
    <%=Html.LpoGaugeDataHidden("GaugeData",Model)%>
</div>
