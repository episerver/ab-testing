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
 "dojo/text!marketing-testing/views/Archive.html",
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

        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
            me.subscribe("/epi/shell/context/changed", me._contextChanged);
        },

        postCreate: function () {
            textHelper.initializeHelper(this.context, resources.archiveview);
            this._renderData();
        },

        startup: function () {
            textHelper.displayPieChart("controlArchivePieChart", textHelper.publishedPercent);
            textHelper.displayPieChart("challengerArchivPieChart", textHelper.draftPercent);
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            textHelper.initializeHelper(this.context, resources.archiveview);

            me._renderData();
            textHelper.displayPieChart("controlArchivePieChart", textHelper.publishedPercent);
            textHelper.displayPieChart("challengerArchivPieChart", textHelper.draftPercent);
        },

        _onCloseClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            this.store = dependency.resolve("epi.storeregistry").get("marketing.abtesting");
            this.topic = this.topic || topic;

            textHelper.renderTitle(this.title);
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
            this.renderStatus();
            this.renderTestDuration();
        },

        renderStatus: function () {
            this.testStatus.innerText = "This test was completed " +
                datetime.toUserFriendlyString(this.context.data.test.endDate) +
                ", " +
                "and content has been chosen to publish.";
        },

        renderTestDuration: function () {
            this.testDuration.innerText = this.context.data.daysElapsed;
            this.testStartDate.innerText = datetime.toUserFriendlyString(this.context.data.test.startDate);
            this.testEndDate.innerText = datetime.toUserFriendlyString(this.context.data.test.endDate);
        },

        renderStatusIndicatorStyles: function () {
            var draftVersion = this.context.data.draftVersionContentLink.split("_")[1];
            var winningVersion = this.context.data.test.variants.find(function (obj) { return obj.isWinner });

            if (draftVersion == winningVersion.itemVersion) {
                this.controlHeader.innerText = resources.archiveview.content_control_header;
                this.challengerHeader.innerText = resources.archiveview.content_challenger_header_picked;
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, "pickedContent");
                domClass.replace(this.controlWrapper, "cardWrapper 2column controlTrailingBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column challengerPublishedBody");
            } else {
                this.controlHeader.innerText = resources.archiveview.content_control_header_picked;
                this.challengerHeader.innerText = resources.archiveview.content_challenger_header;
                domClass.replace(this.controlStatusIcon, "pickedContent");
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, "cardWrapper 2column controlPublishedBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column challengerDefaultBody");
            }
        }
    });
});