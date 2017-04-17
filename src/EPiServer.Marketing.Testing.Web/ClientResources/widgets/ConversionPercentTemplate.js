define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!./templates/ConversionPercentTemplate.html",
    "epi/i18n!marketing-testing/nls/abtesting",
    "dojo/dom-class",
    "xstyle/css!marketing-testing/css/KpiWidget.css"
], function (declare, _WidgetBase, _TemplatedMixin, template, resources, domClass) {

    return declare("ConversionPercentTemplate", [_WidgetBase, _TemplatedMixin], {

        templateString: template,

        resources: resources,

        conversionPercent: null,

        views: null,

        isLeader: null,

        _setIsLeaderAttr: function (leader) {
            if (leader) {
                domClass.replace(this.percentageNode, "epi-kpiSummary-conversionRate-leader");
                domClass.replace(this.viewsNode, "epi-kpiSummary-conversionRate-leader");
            } else {
                domClass.replace(this.percentageNode, "epi-kpiSummary-conversionRate-default");
                domClass.replace(this.viewsNode, "epi-kpiSummary-conversionRate-default");
            }
        }
    });
});