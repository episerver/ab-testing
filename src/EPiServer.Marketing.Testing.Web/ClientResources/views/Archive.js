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
 "dojo/text!marketing-testing/views/Archive.html",
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
        kpiSummaryWidgets: new Array(),


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
            for (var x = 0; x < this.kpiSummaryWidgets.length; x++) {
                this.kpiSummaryWidgets[x].startup();
            }
        },

        _contextChanged: function (newContext) {
            var me = this;
            this.kpiSummaryWidgets = new Array();
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            textHelper.initializeHelper(this.context, resources.archiveview);
            me._renderData();
        },

        _onCloseClick: function () {
            var me = this;
            this.kpiSummaryWidgets = new Array();
            me.contextParameters = { uri: "epi.cms.contentdata:///" + me.context.data.latestVersionContentLink };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var me = this;
            this.store = dependency.resolve("epi.storeregistry").get("marketing.abtesting");
            this.topic = this.topic || topic;
            me._renderStatusIndicatorStyles();
            textHelper.renderTitle(this.title);
            textHelper.renderConfidence(this.confidence);
            textHelper.renderPublishedInfo(this.publishedBy, this.datePublished);
            textHelper.renderDraftInfo(this.changedBy, this.dateChanged);
            this.kpiSummaryWidgets.push(textHelper.renderControlSummary(this.controlArchiveSummaryNode));
            this.kpiSummaryWidgets.push(textHelper.renderChallengerSummary(this.challengerArchiveSummaryNode));
            textHelper.renderDescription(this.testDescription);
            textHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
            this._renderStatus();
            this._renderTestDuration();
            ready(function () {
                me._generateThumbnail(me.context.data.publishPreviewUrl, 'publishThumbnailarchive', 'versiona');
                me._generateThumbnail(me.context.data.draftPreviewUrl, 'draftThumbnailarchive', 'versionb');
                me._renderKpiMarkup("archive_conversionMarkup", "archive_kpidescription");
                for (x = 0; x < me.kpiSummaryWidgets.length; x++) {
                    me.kpiSummaryWidgets[x].startup();
                }
            });

        },

        _renderKpiMarkup: function (conversionMarkupId, kpidescriptionId) {
            var kpiuiElement = dom.byId(conversionMarkupId);
            this._clearKpiMarkup(kpiuiElement);
            new ContentPane({
                content: this.context.data.test.kpiInstances[0].uiReadOnlyMarkup
            }).placeAt(kpiuiElement);

            var kpidescriptionElement = dom.byId(kpidescriptionId);
            this._clearKpiDescription(kpidescriptionElement);
            new ContentPane({
                content: this.context.data.test.kpiInstances[0].description
            }).placeAt(kpidescriptionElement);
        },

        _clearKpiMarkup: function (conversionMarkupElement) {
            if (conversionMarkupElement) {
                var contentPane = dojo.query('#archive_conversionMarkup > *');
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

        _clearKpiDescription: function (conversionMarkupElement) {
            if (conversionMarkupElement) {
                var contentPane = dojo.query('#archive_kpidescription > *');
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

        _renderStatus: function () {
            this.testStatus.innerText = resources.archiveview.test_status_completed + " " + resources.archiveview.content_chosen;
        },

        _renderTestDuration: function () {
            this.testDuration.innerText = this.context.data.daysElapsed;
            this.testStartDate.innerText = datetime.toUserFriendlyString(this.context.data.test.startDate);
            this.testEndDate.innerText = datetime.toUserFriendlyString(this.context.data.test.endDate);
        },

        _renderStatusIndicatorStyles: function () {
            var draftVersion = this.context.data.draftVersionContentLink.split("_")[1];
            var winningVersion = this.context.data.test.variants.find(function (obj) { return obj.isWinner });

            this.controlHeader.innerText = resources.archiveview.content_control_header;
            this.challengerHeader.innerText = resources.archiveview.content_challenger_header;

            if (draftVersion == winningVersion.itemVersion) {
                this.controlVersionTestResult.innerText = resources.archiveview.losing_version_label;
                this.challengerVersionTestResult.innerText = resources.archiveview.winning_version_label;
                this.challengerStatusIcon.title = resources.archiveview.content_selected;
                this.controlStatusIcon.title = "";
                domClass.replace(this.challengerVersionTestResult, "abWinnerStatusText");
                domClass.replace(this.controlVersionTestResult, "abLoserStatusText");
                domClass.replace(this.controlStatusIcon, "noIndicator");
                domClass.replace(this.challengerStatusIcon, "winningContent");
                domClass.replace(this.controlWrapper, "cardWrapper 2column epi-abtest-preview-left-side controlTrailingBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column epi-abtest-preview-right-side challengerPublishedBody");
                query("#publishThumbnailarchive").addClass("epi-abtest-thumbnail--losing");
                query("#draftThumbnailarchive").removeClass("epi-abtest-thumbnail--losing");
            } else {
                this.controlVersionTestResult.innerText = resources.archiveview.winning_version_label;
                this.challengerVersionTestResult.innerText = resources.archiveview.losing_version_label;
                this.controlStatusIcon.title = resources.archiveview.content_selected;
                this.challengerStatusIcon.title = ""
                domClass.replace(this.challengerVersionTestResult, "abLoserStatusText");
                domClass.replace(this.controlVersionTestResult, "abWinnerStatusText");
                domClass.replace(this.controlStatusIcon, "winningContent");
                domClass.replace(this.challengerStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, "cardWrapper 2column epi-abtest-preview-left-side controlPublishedBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column epi-abtest-preview-right-side challengerDefaultBody");
                query("#publishThumbnailarchive").removeClass("epi-abtest-thumbnail--losing");
                query("#draftThumbnailarchive").addClass("epi-abtest-thumbnail--losing");
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
        }
    });
});