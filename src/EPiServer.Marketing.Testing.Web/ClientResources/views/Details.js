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
 "dojo/text!marketing-testing/views/Details.html",
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
            if (this.context.data.kpiResultType === "KpiConversionResult") {
                textHelper.displayPieChart("controlPieChart", textHelper.publishedPercent);
                textHelper.displayPieChart("challengerPieChart", textHelper.draftPercent);
            }
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            this._displayOptionsButton(this.context.data.userHasPublishRights);
            textHelper.initializeHelper(me.context, resources.detailsview);
            me._renderData();
            //redraw the charts when the context changes to update the stored dom.
            if (this.context.data.kpiResultType === "KpiConversionResult") {
                textHelper.displayPieChart("controlPieChart", textHelper.publishedPercent);
                textHelper.displayPieChart("challengerPieChart", textHelper.draftPercent);
            }
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
                uri: "epi.cms.contentdata:///" + this.context.data.draftVersionContentLink
            };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = {
                uri: "epi.cms.contentdata:///" + this.context.data.latestVersionContentLink
            };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onControlViewClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onChallengerViewClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.draftVersionContentLink };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var me = this;
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
            ready(function () {
                me._generateThumbnail(me.context.data.publishPreviewUrl, 'publishThumbnaildetail', 'versiona');
                me._generateThumbnail(me.context.data.draftPreviewUrl, 'draftThumbnaildetail', 'versionb');
                me._renderKpiMarkup("details_conversionMarkup");
            });
            this.renderStatusIndicatorStyles();
        },

        _renderKpiMarkup: function (conversionMarkupId) {
            var kpiuiElement = dom.byId(conversionMarkupId);
            this._clearKpiMarkup(kpiuiElement);
            new ContentPane({
                content: this.context.data.test.kpiInstances[0].uiReadOnlyMarkup
            }).placeAt(kpiuiElement);
        },

        _clearKpiMarkup: function (conversionMarkupElement) {
            if (conversionMarkupElement) {
                var contentPane = dojo.query('#details_conversionMarkup > *');
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

        _displayOptionsButton: function (show) {
            var dropDownButton = registry.byId("optionsDropdown");
            var pickWinnerOption = registry.byId("pickWinnerMenuItem");
            if (show) {
                //If the test is not running, disable the pick a winner option item
                if (this.context.data.test.state === 0) {
                    pickWinnerOption.set("disabled", true);
                } else {
                    pickWinnerOption.set("disabled", false);
                }
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

            if (textHelper.publishedPercent > textHelper.draftPercent) {
                this.controlStatusIcon.title = resources.detailsview.content_winning_tooltip;
                this.challengerStatusIcon.title = "";
                domClass.replace(this.controlStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column epi-abtest-preview-left-side controlLeaderBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column epi-abtest-preview-right-side challengerDefaultBody");
            }
            else if (textHelper.publishedPercent < textHelper.draftPercent) {
                this.controlStatusIcon.title = "";
                this.challengerStatusIcon.title = resources.detailsview.content_winning_tooltip;
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, me.statusIndicatorClass);
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column epi-abtest-preview-left-side controlTrailingBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column epi-abtest-preview-right-side challengerLeaderBody");
            }
            else {
                this.controlStatusIcon.title = "";
                this.challengerStatusIcon.title = "";
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, me.baseWrapper + " 2column epi-abtest-preview-left-side controlDefaultBody");
                domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column epi-abtest-preview-right-side challengerDefaultBody");
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