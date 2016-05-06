define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!marketing-testing/views/MarketingTestingDetailsView.html",
    "dijit/_WidgetsInTemplateMixin"
    
], function (
    declare,
    widgetBase,
    templatedMixin,
    template,
    widgetsInTemplateMixin

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin], {

        templateString: template
    });
});