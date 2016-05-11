define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!marketing-testing/views/MarketingTestDetailsView.html",
    "dijit/_WidgetsInTemplateMixin"
    
], function (
    declare,
    widgetBase,
    templatedMixin,
    template,
    widgetsInTemplateMixin

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin], {

        templateString: template,
        contextData: null,

        constructor: function () {
            var contextService = epi.dependency.resolve("epi.shell.ContextService"), me = this;
            this.contextData = contextService.currentContext;
        }
    });
});