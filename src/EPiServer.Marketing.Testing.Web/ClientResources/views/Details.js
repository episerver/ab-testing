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
  "dojo/dom-class",
 "marketing-testing/scripts/abTestTextHelper",
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
    textHelper
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
            textHelper.initializeHelper(this.context, resources.detailsview);
            this._renderData();
        },

        startup: function () {
            this._displayOptionsButton(this.context.data.userHasPublishRights);
            //make the charts at start up as the dom is not ready for it prior to this on 
            //the first load.
            textHelper.displayPieChart("controlPieChart", textHelper.publishedPercent);
            textHelper.displayPieChart("challengerPieChart", textHelper.draftPercent);
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            textHelper.initializeHelper(me.context, resources.detailsview);
            me._renderData();
            //redraw the charts when the context changes to update the stored dom.
            textHelper.displayPieChart("controlPieChart", textHelper.publishedPercent);
            textHelper.displayPieChart("challengerPieChart", textHelper.draftPercent);
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
            textHelper.renderTitle(this.title);
            textHelper.renderTestStatus(this.testStatus, this.testStarted);
            textHelper.renderTestDuration(this.testDuration);
            textHelper.renderTestRemaining(this.testRemaining);
            textHelper.renderConfidence(this.confidence);
            textHelper.renderPublishedInfo(this.publishedBy, this.datePublished);
            textHelper.renderDraftInfo(this.changedBy, this.dateChanged);
            textHelper.renderPublishedViewsAndConversions(this.publishedConversions,
                this.publishedViews,
                this.publishedConversionPercent);
            textHelper.renderDraftViewsAndConversions(this.challengerConversions,
                this.challengerViews,
                this.challengerConversionPercent);
            textHelper.renderDescription(this.testDescription);
            textHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
            textHelper.renderConversion(this.contentLinkAnchor);
            this.renderStatusIndicatorStyles();
        },

        _displayOptionsButton: function (show) {
            var dropDownButton = registry.byId("optionsDropdown");
            if (show) {
                domStyle.set(dropDownButton.domNode, "visibility", "visible");
                dropDownButton.startup(); //Avoids conditions where the widget is rendered but not active.
            } else {
                domStyle.set(dropDownButton.domNode, "visibility", "hidden");
            }
        },

        renderStatusIndicatorStyles: function () {
            var me = this;
            me.baseWrapper = "cardWrapper";
            if (this.context.data.test.state < 2) {
                me.statusIndicatorClass = "leadingContent";
            }
            else { me.statusIndicatorClass = "winningContent"; }

            if (this.publishedPercent > this.draftPercent) {
                domClass.replace(this.controlStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column controlLeaderBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column challengerDefaultBody");
            }
            else if (this.publishedPercent < this.draftPercent) {
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column controlTrailingBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column challengerLeaderBody");
            }
            else {
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column controlDefaultBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column challengerDefaultBody");
            }
        }
    });
});