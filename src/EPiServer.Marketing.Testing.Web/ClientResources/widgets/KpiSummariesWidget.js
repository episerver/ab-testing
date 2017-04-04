define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/KpiSummariesWidget.html",
    "epi/i18n!marketing-testing/nls/abtesting",
    "dojo/dom",
    "dojo/dom-construct",
    "marketing-testing/widgets/WeightSelector",
    "xstyle/css!marketing-testing/css/KpiWidget.css"
], function (declare, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, template, resources, dom, domConstruct, weightSelector) {

    return declare("KpiSummariesWidget", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        id: this.id,

        templateString: template,

        resources: resources,

        //requires being set to an array of objects
        //objects need to contain name, conversion, weight (Low, Medium, High), and performance value
        kpis: null,

        startup: function () {
            var trInsertNode = dom.byId(this.id + "Summaries")
            if (trInsertNode) {
                trInsertNode.innerHTML = "";
                for (var x = 0; x < this.kpis.length; x++) {
                    var row = domConstruct.toDom("<tr class='epi-kpiSummaries-row'><td'>" + this.kpis[x].name +
                        "</td><td class='epi-kpiSummaries-data'>" + this.kpis[x].conversions +
                        "</td><td class='epi-kpiSummaries-data' id='" + this.id + "weight" + x + "'>" +
                        "</td><td class='epi-kpiSummaries-data'>" + this.kpis[x].performance + "</td></tr>");
                    domConstruct.place(row, trInsertNode);
                    new weightSelector({
                        value: this.kpis[x].weight,
                        disabled: true,
                        showLabel: false
                    }).placeAt(this.id + "weight" + x);
                }
            }
        }
    });
});