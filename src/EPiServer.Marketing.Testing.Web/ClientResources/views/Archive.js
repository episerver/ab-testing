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
    query,
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
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split("_")[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var me = this;
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
            this.renderKpiUi();
            this.renderStatusIndicatorStyles();
            this.renderStatus();
            this.renderTestDuration();

            ready(function () {
                me._generateThumbnail(me.context.data.publishPreviewUrl, 'publishThumbnailarchive', 'versiona');
                me._generateThumbnail(me.context.data.draftPreviewUrl, 'draftThumbnailarchive', 'versionb');
            });

        },

        renderKpiUi: function () {
            if (this.kpiMarkup) {
                this.kpiMarkup.innerHTML = this.context.data.test.kpiInstances[0].uiReadOnlyMarkup;
            }
        },

        renderStatus: function () {
            this.testStatus.innerText = resources.archiveview.test_status_completed +
                " " +
                datetime.toUserFriendlyString(this.context.data.test.endDate) +
                resources.archiveview.content_chosen;
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