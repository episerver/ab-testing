define([
 "dojo/_base/declare",
 "epi/dependency",
 "dojo/dom",
 "dijit/registry",
 "dojo/dom-style",
 "dojo/topic",
 "dijit/_WidgetBase",
 "dijit/_TemplatedMixin",
 "dijit/_WidgetsInTemplateMixin",
 "dojo/text!marketing-testing/views/PickWinner.html",
 "epi/i18n!marketing-testing/nls/abtesting",
 "epi/datetime",
 "epi/username",
 "dojo/dom-class",
 "../scripts/abUIHelper",
 "xstyle/css!marketing-testing/css/ABTesting.css",
 "xstyle/css!marketing-testing/css/GridForm.css",
 "xstyle/css!marketing-testing/css/dijit.css",
 "dijit/form/DropDownButton",
 "dijit/TooltipDialog",
 "dijit/form/Button"

], function (
    declare,
    dependency,
    dom,
    registry,
    domStyle,
    topic,
    widgetBase,
    templatedMixin,
    widgetsInTemplateMixin,
    template,
    resources,
    datetime,
    username,
    domClass,
    uiHelper

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin],
    {
        templateString: template,
        resources: resources,
        contextHistory: null,

        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
            me.subscribe("/epi/shell/context/changed", me._contextChanged);
        },

        postCreate: function () {
            uiHelper.initializeHelper(this.context);
            this._renderData();
        },

        startup: function () {
            uiHelper.displayPieChart("controlPickWinnerPieChart", uiHelper.publishedPercent);
            uiHelper.displayPieChart("challengerPickWinnerPieChart", uiHelper.draftPercent);
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            me._renderData();
            uiHelper.displayPieChart("controlPickWinnerPie", uiHelper.publishedPercent);
            uiHelper.displayPieChart("challengerPickWinnerPie", uiHelper.draftPercent);
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            uiHelper.renderTitle(this.title);
            uiHelper.renderTestStatus(this.testStatus, this.testStarted);
            uiHelper.renderTestDuration(this.testDuration);
            uiHelper.renderTestRemaining(this.testRemaining);
            uiHelper.renderConfidence(this.confidence);
            uiHelper.renderPublishedInfo(this.publishedBy, this.datePublished);
            uiHelper.renderDraftInfo(this.changedBy, this.dateChanged);
            uiHelper.renderPublishedViewsAndConversions(this.publishedConversions,
                this.publishedViews,
                this.publishedConversionPercent);
            uiHelper.renderDraftViewsAndConversions(this.challengerConversions,
                this.challengerViews,
                this.challengerConversionPercent);
            uiHelper.renderDescription(this.testDescription);
            uiHelper.renderStatusIndicatorStyles(this.publishedStatusIcon,
                this.variantStatusIcon,
                this.controlWrapper,
                this.challengerWrapper,
                "true");
            uiHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
            uiHelper.renderConversion(this.contentLinkAnchor);
            uiHelper.renderSignificance(this.pickAWinnerMessage);
        },

        _onPublishedVersionClick: function () {
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.publishedVersionContentLink,
                testId: this.context.data.test.id
            }, { id: this.context.data.test.id }, { "options.incremental": false })  // Force a put
                    .then(function (data) {
                        var contextParameters = { uri: "epi.cms.contentdata:///" + data };
                        topic.publish("/epi/shell/context/request", contextParameters);
                    }).otherwise(function () {
                        alert("Error Processing Winner: Unable to process and save selected version");
                        console.log("Error occurred while processing winning content");
                    });
        },

        _onVariantVersionClick: function () {
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.draftVersionContentLink,
                testId: this.context.data.test.id
            }, { id: this.context.data.test.id }, { "options.incremental": false }) // Force a put
                .then(function (data) {
                    var contextParameters = { uri: "epi.cms.contentdata:///" + data };
                    topic.publish("/epi/shell/context/request", contextParameters);
                }).otherwise(function () {
                    alert("Error Processing Winner: Unable to process and save selected version");
                    console.log("Error occurred while processing winning content");
                });
        }
    });
});