define([
    "dojo/_base/declare",
    "dojo/Evented",
    "dijit/_WidgetBase",
    "dijit/_OnDijitClickMixin",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/KpiWeightWidget.html",
    'dojo/dom',
    'xstyle/css!marketing-testing/css/KpiWidget.css',


], function (declare, Evented, _WidgetBase, _OnDijitClickMixin, _TemplatedMixin, _WidgetsInTemplateMixin, template, dom) {

    return declare("KpiWeightWidget", [Evented, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        templateString: template,

        label: "",

        id: this.id,

        postCreate: function () {

        }
    });
});