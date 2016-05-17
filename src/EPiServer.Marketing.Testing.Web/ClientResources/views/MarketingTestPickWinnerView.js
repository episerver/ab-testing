define([
 'dojo/_base/declare',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dojo/text!marketing-testing/views/MarketingTestPickWinnerView.html'

], function (
    declare,
    _WidgetBase,
    _TemplatedMixin,
    template
) {
    return declare([_WidgetBase, _TemplatedMixin], {
        templateString: template,
    });
});