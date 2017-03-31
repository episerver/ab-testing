define([
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
    "dijit/form/Form",
    'xstyle/css!marketing-testing/css/KpiWidget.css',


], function (declare, Evented, _WidgetBase, _OnDijitClickMixin, _TemplatedMixin, _WidgetsInTemplateMixin, template, dom, ContentPane) {

    return declare("KpiWidget", [Evented, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        templateString: template,

        label: "",

        description: "",

        markup: "",

        kpiType: "",

        id: this.id,

        linkedWidgetId: null,

        postCreate: function () {
            new ContentPane({
                content: this.markup
            }).placeAt(this.kpiMarkup);
            this._getCurrentContent();
        },

        _setImportanceAttr: function (value) {
            this.kpiWeight.value = value;
        },

        _setlinkedWidgetIdAttr: function (value) {
            this.linkedWidgetId = value;
        },

        removeWidget: function () {
            var widget = dijit.byId(this.id);
            var linkedWidget = dijit.byId(this.linkedWidgetId);
            linkedWidget.destroy();
            widget.destroy();
            document.dispatchEvent(destroyedEvent);
        },

        _getCurrentContent: function () {
            var dependency = require("epi/dependency")
            var contextService = dependency.resolve("epi.shell.ContextService");
            var context = contextService.currentContext;
            this.CurrentContent.value = context.id;
        }
    });
});