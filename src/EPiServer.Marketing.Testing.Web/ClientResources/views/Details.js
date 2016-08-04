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
 "dojo/text!marketing-testing/views/Details.html",
 "epi/i18n!marketing-testing/nls/abtesting",
 "epi/datetime",
 "epi/username",
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
    uiHelper
) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin],
    {
        templateString: template,
        resources: resources,
        contextHistory: null,
        controlPercentage: null,
        challengerPercentage: null,

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
            this._displayOptionsButton(this.context.data.userHasPublishRights);
            uiHelper.displayPieChart("controlPieChart", uiHelper.publishedPercent);
            uiHelper.displayPieChart("challengerPieChart", uiHelper.draftPercent);
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            uiHelper.initializeHelper(me.context);
            me._renderData();
            uiHelper.displayPieChart("controlPieChart", uiHelper.publishedPercent);
            uiHelper.displayPieChart("challengerPieChart", uiHelper.draftPercent);
        },

        _onPickWinnerOptionClicked: function () {
            var me = this;
            me.contextParameters = {
                uri: "epi.marketing.testing:///testid=" + this.context.data.test.id + "/PickWinner"
            };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onAbortOptionClicked: function () {
            var me = this, store = this.store || dependency.resolve("epi.storeregistry").get("marketing.abtesting");
            store.remove(this.context.data.test.originalItemId);
            me.contextParameters = {
                uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0]
            };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = {
                uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0]
            };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            uiHelper.renderTitle(this.titleNode);
            uiHelper.renderTestStatus(this.testStatusNode, this.testStartedNode);
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
                this.challengerWrapper);
            uiHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
            uiHelper.renderConversion(this.contentLinkAnchor);
        },

        _displayOptionsButton: function (show) {
            var dropDownButton = registry.byId("optionsDropdown");
            if (show) {
                domStyle.set(dropDownButton.domNode, "visibility", "visible");
                dropDownButton.startup(); //Avoids conditions where the widget is rendered but not active.
            } else {
                domStyle.set(dropDownButton.domNode, "visibility", "hidden");
            }
        }
    });
});