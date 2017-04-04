define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/KpiSummaryWidget.html",
    "epi/i18n!marketing-testing/nls/abtesting",
    "dojo/dom",
    "dojo/dom-class",
    "dojox/charting/Chart",
    "dojox/charting/plot2d/Pie",
    "xstyle/css!marketing-testing/css/KpiWidget.css"
], function (declare, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, template, resources, dom, domClass, chart, pie) {

    return declare("KpiSummaryWidget", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        id: this.id,

        templateString: template,

        resources: resources,

        conversionRate: null,

        conversions: null,

        views: null,

        isLeader: false,

        postCreate: function () {
            if (this.isLeader) {
                domClass.replace(this.conversionPercent, "epi-kpiSummary-conversionRate-leader")
            }
        },

        startup: function () {
            this.displayPieChart("pieChart" + this.id);
        },

        displayPieChart: function (node) {
            if (dom.byId(node)) {
                dom.byId(node).innerHTML = "";

                var chartNode = dom.byId(node);
                var pieChart = new chart(chartNode);

                var chartData = [
                    {
                        x: 1,
                        y: 100 - this.conversionRate,
                        fill: "#edebe9"
                    }, {
                        x: 1,
                        y: this.conversionRate,
                        fill: "#86c740"
                    }
                ];

                pieChart.addPlot("default",
                {
                    type: "Pie",
                    labels: false,
                    radius: 50
                });
                pieChart.addSeries("", chartData, { stroke: { width: 0 } });
                pieChart.render();
            }
        },
    });
});