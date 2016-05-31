define([
 'dojo/_base/declare',
     'epi/dependency',

    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dojo/text!marketing-testing/views/PickWinner.html',
    'dojo/i18n!marketing-testing/nls/MarketingTestingLabels'


], function (
    declare,
    dependency,
    _WidgetBase,
    _TemplatedMixin,
    template
) {
    return declare([_WidgetBase, _TemplatedMixin], {
        templateString: template,
        resources: resources,
        contextHistory: null,


        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;

        },

        postcreate: function() {
            this.contextHistory = dependency.resolve("epi.cms.BackContextHistory");

        },

        _onCloseButtonClick: function () {
            this.contextHistory.closeAndNavigateBack(this);
        }



    });
});