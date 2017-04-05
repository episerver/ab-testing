define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!./templates/KpiSummaryRow.html",
    "epi/i18n!marketing-testing/nls/abtesting",
    "dojo/dom-construct",
    "xstyle/css!marketing-testing/css/KpiWidget.css"
], function (declare, _WidgetBase, _TemplatedMixin, template, domConstruct) {

    return declare("KpiSummaryRow", [_WidgetBase, _TemplatedMixin], {

        id: this.id,

        templateString: template,

        name: null,

        conversions: null,

        performance: null,

    });
});