define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/KpiWeightWidget.html",
    'dojo/dom',
    'xstyle/css!marketing-testing/css/KpiWidget.css',
    'marketing-testing/widgets/WeightSelector'
], function (declare, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, template, dom) {
    return declare("KpiWeightWidget", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        templateString: template,

        label: "",

        id: this.id,

        kpiWidgetId: null,

        _setValueAttr: function (value) {
            var kpiWidget = dijit.byId(this.kpiWidgetId);
            kpiWidget._setImportanceAttr(value);
        },

        postCreate: function () {
            this.weightSelector._setValueAttr(this.value);
            var kpiWidget = dijit.byId(this.kpiWidgetId);
            kpiWidget._setImportanceAttr(this.value);
        },

        _setWeight: function () {
            this._setValueAttr(this.weightSelector.value);
        }


    });
});