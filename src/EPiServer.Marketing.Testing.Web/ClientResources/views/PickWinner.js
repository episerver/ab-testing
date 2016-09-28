define([
 "dojo/_base/declare",
 "epi/dependency",
 "dojo/dom",
 "dojo/ready",
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
 "marketing-testing/scripts/abTestTextHelper",
  "marketing-testing/scripts/rasterizeHTML",
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
    ready,
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
    textHelper,
    rasterizehtml

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
            textHelper.initializeHelper(this.context, resources.pickwinnerview);
            this._renderData();
        },

        startup: function () {
            textHelper.displayPieChart("controlPickWinnerPieChart", textHelper.publishedPercent);
            textHelper.displayPieChart("challengerPickWinnerPieChart", textHelper.draftPercent);
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            textHelper.initializeHelper(this.context, resources.pickwinnerview);

            me._renderData();
            textHelper.displayPieChart("controlPickWinnerPie", textHelper.publishedPercent);
            textHelper.displayPieChart("challengerPickWinnerPie", textHelper.draftPercent);
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var me = this;
            this.store = dependency.resolve("epi.storeregistry").get("marketing.abtesting");
            this.topic = this.topic || topic;

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
            textHelper.renderSignificance(this.pickAWinnerMessage);

            ready(function () {
                me._generateThumbnail(me.context.data.publishPreviewUrl, 'publishThumbnailpickwinner');
                me._generateThumbnail(me.context.data.draftPreviewUrl, 'draftThumbnailpickwinner');
            });
            this.renderStatusIndicatorStyles();
        },

        _onPublishedVersionClick: function () {
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.publishedVersionContentLink,
                testId: this.context.data.test.id
            }, { id: this.context.data.test.id }, { "options.incremental": false })  // Force a put
                    .then(function (testId) {
                        var contextParameters = { uri: "epi.marketing.testing:///testid=" + testId + "/Archive" };
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
                .then(function (testId) {
                    var contextParameters = { uri: "epi.marketing.testing:///testid=" + testId + "/Archive" };
                    topic.publish("/epi/shell/context/request", contextParameters);
                }).otherwise(function () {
                    alert("Error Processing Winner: Unable to process and save selected version");
                    console.log("Error occurred while processing winning content");
                });
        },

        renderStatusIndicatorStyles: function () {
            var me = this;
            me.baseWrapper = "cardWrapperShadowed";
            if (this.context.data.test.state < 2) {
                me.statusIndicatorClass = "leadingContent";
            }
            else { me.statusIndicatorClass = "winningContent"; }

            if (textHelper.publishedPercent > textHelper.draftPercent) {
                domClass.replace(this.controlStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column controlLeaderBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column challengerDefaultBody");
            }
            else if (textHelper.publishedPercent < textHelper.draftPercent) {
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
        },
        _generateThumbnail: function (previewUrl, canvasId) {
            var pubThumb = dom.byId(canvasId);

            if (pubThumb) {
                pubThumb.height = 768;
                pubThumb.width = 1024;
                rasterizehtml.drawURL(previewUrl, pubThumb, { height: 768, width: 1024 });
            }
        }
    });
});