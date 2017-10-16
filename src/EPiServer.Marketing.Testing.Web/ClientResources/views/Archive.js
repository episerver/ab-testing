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
 "marketing-testing/scripts/thumbnails",
 "dojox/layout/ContentPane",
 "dojo/fx",
 "dojo/dom-construct",
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
    thumbnails,
    ContentPane,
    CoreFX,
    DomConstruct
) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin],
    {
        templateString: template,
        resources: resources,
        contextHistory: null,
        kpiSummaryWidgets: new Array(),

        startup: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            this.context = contextService.currentContext;
            textHelper.initializeHelper(this.context, resources.archiveview);
            this._resetView();
            this._renderData();
        },

        _setToggleAnimations: function () {
            var me = this;
            this.controlSummaryOut = CoreFX.wipeOut({
                node: me.controlArchiveSummaryNode,
                rate: 15,
                onBegin: function () { me.summaryToggle.innerHTML = me.resources.archiveview.show_summary }
            });

            this.controlSummaryIn = CoreFX.wipeIn({
                node: me.controlArchiveSummaryNode,
                rate: 15,
                onBegin: function () { me.summaryToggle.innerHTML = me.resources.archiveview.hide_summary }
            });

            this.challengerSummaryOut = CoreFX.wipeOut({
                node: me.challengerArchiveSummaryNode,
                rate: 15
            });

            this.challengerSummaryIn = CoreFX.wipeIn({
                node: me.challengerArchiveSummaryNode,
                rate: 15
            });
        },

        _onCloseClick: function () {
            var me = this;
            this.kpiSummaryWidgets = new Array();
            me.contextParameters = { uri: "epi.cms.contentdata:///" + me.context.data.latestVersionContentLink };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var me = this;
            this.kpiSummaryWidgets = new Array();
            this.controlArchiveSummaryNode.style.display = "block";
            this.challengerArchiveSummaryNode.style.display = "block";
            me.summaryToggle.innerHTML = me.resources.archiveview.hide_summary
            me._renderStatusIndicatorStyles();
            textHelper.renderTitle(this.title);
            textHelper.renderConfidence(this.confidence);
            textHelper.renderPublishedInfo(this.publishedBy, this.datePublished);
            textHelper.renderDraftInfo(this.changedBy, this.dateChanged);
            this.kpiSummaryWidgets.push(textHelper.renderControlSummary(this.controlArchiveSummaryNode, this.controlConversionPercent));
            this.kpiSummaryWidgets.push(textHelper.renderChallengerSummary(this.challengerArchiveSummaryNode, this.challengerConversionPercent));
            textHelper.renderDescription(this.testDescription);
            textHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
            this._renderStatus();
            this._renderTestDuration();

            ready(function () {
                pubThumb = document.getElementById("publishThumbnailarchive");
                draftThumb = document.getElementById("draftThumbnailarchive");
                if (me.context.customViewType == "marketing-testing/views/Archive") {
                    thumbnails._setThumbnail(pubThumb, me.context.data.publishPreviewUrl);
                    thumbnails._setThumbnail(draftThumb, me.context.data.draftPreviewUrl);
                };


                me._renderKpiMarkup("archive_conversionMarkup");
                for (x = 0; x < me.kpiSummaryWidgets.length; x++) {
                    me.kpiSummaryWidgets[x].startup();
                }
                if (me.context.data.test.kpiInstances.length > 1) {
                    me._setToggleAnimations();
                    me.summaryToggle.style.visibility = "visible";
                } else {
                    me.summaryToggle.style.visibility = "hidden";
                }
            });
        },

        _renderKpiMarkup: function (conversionMarkupId) {
            var kpiuiElement = dom.byId(conversionMarkupId);
            this._clearKpiMarkup(kpiuiElement);

            for (var x = 0; x < this.context.data.test.kpiInstances.length; x++) {
                var goalsFriendlyName = DomConstruct.toDom("<label class='epi-kpiLabel-bold'>" + this.context.data.test.kpiInstances[x].friendlyName + "</label>");
                var goalsDescription = DomConstruct.toDom("<P>" + this.context.data.test.kpiInstances[x].description + "</p>");

                var goalsContent = new ContentPane({
                    content: this.context.data.test.kpiInstances[x].uiReadOnlyMarkup
                }).placeAt(kpiuiElement);
                dojo.place(goalsFriendlyName, goalsContent.containerNode, "first");
                dojo.place(goalsDescription, goalsContent.containerNode, "last");
            }
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

        _toggleSummaries: function () {
            if (this.summaryToggle.innerHTML === this.resources.archiveview.hide_summary) {
                this.controlSummaryOut.play();
                this.challengerSummaryOut.play();
            }
            else {
                this.controlSummaryIn.play();
                this.challengerSummaryIn.play();
            }
        },

        _resetView: function () {
            var abTestBody = dom.byId("abTestBody");
            var toolbarGroup = dom.byId("toolbarGroup");
            if (abTestBody) {
                abTestBody.scrollIntoView(true);
                toolbarGroup.scrollIntoView(true);
            }
        },
    });
});