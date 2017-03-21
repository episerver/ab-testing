﻿define([
    "dojo/_base/declare",
    "dojo/Evented",
    "dijit/_WidgetBase",
    "dijit/_OnDijitClickMixin",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/KpiWidget.html",
    'dojo/dom',
    "dojox/layout/ContentPane",
    "dijit/form/Button",
    'xstyle/css!marketing-testing/css/KpiWidget.css',


], function (declare, Evented, _WidgetBase, _OnDijitClickMixin, _TemplatedMixin, _WidgetsInTemplateMixin, template, dom, ContentPane) {

    return declare("KpiWidget", [Evented, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        templateString: template,

        label: "",

        description: "",

        markup: "",

        kpiType: "",

        postCreate: function () {
            new ContentPane({
                content: this.markup
            }).placeAt(this.kpiMarkup);
        },

        removeWidget: function () {

            var widget = dijit.byId(this.id);
            widget.destroy();
            document.dispatchEvent(destroyedEvent);
        }
    });

});