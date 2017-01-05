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
 "dojo/query",
 "marketing-testing/scripts/abTestTextHelper",
 "marketing-testing/scripts/rasterizeHTML",
 "dojox/layout/ContentPane",
 "xstyle/css!marketing-testing/css/ABTesting.css",
 "dijit/form/DropDownButton",
 "dijit/TooltipDialog",
 "dijit/form/Button",
 "dijit/ProgressBar"

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
    query,
    textHelper,
    rasterizehtml,
    ContentPane

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
            if (this.context.data.test.kpiInstances[0].kpiResultType === "kpiConversionResult") {
                textHelper.displayPieChart("controlPickWinnerPieChart", textHelper.publishedPercent);
                textHelper.displayPieChart("challengerPickWinnerPieChart", textHelper.draftPercent);
            }
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            textHelper.initializeHelper(this.context, resources.pickwinnerview);

            me._renderData();
            if (this.context.data.test.kpiInstances[0].kpiResultType === "kpiConversionResult") {
                textHelper.displayPieChart("controlPickWinnerPie", textHelper.publishedPercent);
                textHelper.displayPieChart("challengerPickWinnerPie", textHelper.draftPercent);
            }
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.latestVersionContentLink };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var me = this;
            this.store = dependency.resolve("epi.storeregistry").get("marketing.abtesting");
            this.topic = this.topic || topic;

            textHelper.renderTitle(this.title);
            textHelper.renderTestStatus(this.testStatus, this.testStarted);
            textHelper.renderTestDuration(this.testDuration);
            textHelper.renderTestRemaining(this.testRemaining, this.testRemainingText);
            textHelper.renderDurationProgress(durationProgressBar);
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
            this._renderSignificance();

            ready(function () {
                me._generateThumbnail(me.context.data.publishPreviewUrl, 'publishThumbnailpickwinner', 'versiona');
                me._generateThumbnail(me.context.data.draftPreviewUrl, 'draftThumbnailpickwinner', 'versionb');
                me._renderStatusIndicatorStyles();
                me._renderKpiMarkup("pw_conversionMarkup");
            });
        },

        _renderKpiMarkup: function (conversionMarkupId) {
            var kpiuiElement = dom.byId(conversionMarkupId);
            var x = this.context.data.test.kpiInstances[0].uiReadOnlyMarkup;
            this._clearKpiMarkup(kpiuiElement);
            new ContentPane({
                content: this.context.data.test.kpiInstances[0].uiReadOnlyMarkup
            }).placeAt(kpiuiElement);
        },

        _clearKpiMarkup: function (conversionMarkupElement) {
            if (conversionMarkupElement) {
                var contentPane = dojo.query('#pw_conversionMarkup > *');
                if (contentPane[0]) {
                    dojo.forEach(dijit.findWidgets(contentPane)), function (w) {
                        w.destroyRecursive();
                    };
                    var dijitContentPane = dijit.byId(contentPane[0].id);
                    dijitContentPane.destroy();
                    conversionMarkupElement.innerHTML = "";
                }
            }
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

        _renderSignificance: function () {
            var pickWinnerMessage = this.pickAWinnerMessage;
            var pickWinnerWarningIcon = this.pickWinnerWarningIcon;
            var currentIconClass = pickWinnerWarningIcon.className;
            var iconClassDisplayed = currentIconClass.replace("dijitHidden", "dijitInline");
            var iconClassHidden = currentIconClass.replace("dijitInline", "dijitHidden")

            if (this.context.data.test.state < 2) {
                pickWinnerMessage.innerHTML = resources.pickwinnerview.early_pick_winner_message;
            } else if (this.context.data.test.state === 2) {
                if (!this.context.data.test.isSignificant) {
                    pickWinnerMessage.innerHTML = resources.pickwinnerview.result_is_not_significant;
                    domClass.replace(pickWinnerWarningIcon, iconClassDisplayed);
                } else {
                    pickWinnerMessage.innerHTML = "";
                    domClass.replace(pickWinnerWarningIcon, iconClassHidden);
                }
            }
        },

        _renderStatusIndicatorStyles: function () {
            var me = this;
            if (this.context.data.test.state < 2) {
                me.statusIndicatorClass = "leadingContent";
            }
            else { me.statusIndicatorClass = "winningContent"; }

            if (textHelper.publishedPercent > textHelper.draftPercent) {
                this.controlStatusIcon.title = resources.pickwinnerview.content_winning_tooltip;
                this.challengerStatusIcon.title = "";
                domClass.replace(this.controlStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlPickWinnerBtn, "epi-success abPickWinnerAction");
                domClass.replace(this.challengerPickWinnerBtn, "abPickWinnerAction");
                query("#publishThumbnailpickwinner").removeClass("epi-abtest-thumbnail--losing");
                query("#draftThumbnailpickwinner").addClass("epi-abtest-thumbnail--losing");
            }
            else if (textHelper.publishedPercent < textHelper.draftPercent) {
                this.controlStatusIcon.title = "";
                this.challengerStatusIcon.title = resources.pickwinnerview.content_winning_tooltip;
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.controlPickWinnerBtn, "abPickWinnerAction");
                domClass.replace(this.challengerPickWinnerBtn, "epi-success abPickWinnerAction");
                query("#publishThumbnailpickwinner").addClass("epi-abtest-thumbnail--losing");
                query("#draftThumbnailpickwinner").removeClass("epi-abtest-thumbnail--losing");
            }
            else {
                this.controlStatusIcon.title = "";
                this.challengerStatusIcon.title = "";
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlPickWinnerBtn, "abPickWinnerAction");
                domClass.replace(this.challengerPickWinnerBtn, "abPickWinnerAction");
                query("#publishThumbnailpickwinner").removeClass("epi-abtest-thumbnail--losing");
                query("#draftThumbnailpickwinner").removeClass("epi-abtest-thumbnail--losing");
            }
        },
        _generateThumbnail: function (previewUrl, canvasId, parentContainerClass) {
            var pubThumb = dom.byId(canvasId);

            if (pubThumb) {
                pubThumb.height = 768;
                pubThumb.width = 1024;
                rasterizehtml.drawURL(previewUrl, pubThumb, { height: 768, width: 1024 }).then(
                    function success(renderResult) {
                        query('.' + parentContainerClass).addClass('hide-bg');
                    });
            }
        }
    });
});