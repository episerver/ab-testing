define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!marketing-testing/views/MultivariateTestDetailsView.html",
    "marketing-testing/viewmodels/TestDetailsViewModel",
    "dijit/_WidgetsInTemplateMixin"
    
], function (
    declare,
    widgetBase,
    templatedMixin,
    template,
    testDetailsViewModel,
    widgetsInTemplateMixin

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin], {

        templateString: template
    });
});