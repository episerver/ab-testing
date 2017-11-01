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
 "marketing-testing/scripts/thumbnails",
 "dojox/layout/ContentPane",
 "dojo/fx",
 "dojo/dom-construct",
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

        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
            me.subscribe("/epi/shell/context/changed", me._contextChanged);
        },

        _contextChanged: function (newContext) {
            var me = this;
            this.kpiSummaryWidgets = new Array();
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            textHelper.initializeHelper(this.context, resources.pickwinnerview);
            if (this.context.data.test.kpiInstances.length > 1) {
                this._setToggleAnimations();
                this.summaryToggle.style.visibility = "visible"
            } else {
                this.summaryToggle.style.visibility = "hidden"
            }
            this._renderData();
            this._resetView();
        },

        startup: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
            textHelper.initializeHelper(this.context, resources.pickwinnerview);

            if (document.getElementById("draftThumbnailpickwinner")) {
                document.getElementById("publishThumbnailpickwinner-spinner").style.display = "block";
                document.getElementById("draftThumbnailpickwinner-spinner").style.display = "block";
                document.getElementById("publishThumbnailpickwinner").style.display = "none";
                document.getElementById("draftThumbnailpickwinner").style.display = "none";
            }
            if (this.context.data.test.kpiInstances.length > 1) {
                this._setToggleAnimations();
                this.summaryToggle.style.visibility = "visible"
            } else {
                this.summaryToggle.style.visibility = "hidden"
            }
            this._resetView();
            this._renderData();
        },

        _setToggleAnimations: function () {
            var me = this;
            this.controlSummaryOut = CoreFX.wipeOut({
                node: me.controlPickWinnerSummaryNode,
                rate: 15,
                onBegin: function () { me.summaryToggle.innerHTML = me.resources.pickwinnerview.show_summary }
            });

            this.controlSummaryIn = CoreFX.wipeIn({
                node: me.controlPickWinnerSummaryNode,
                rate: 15,
                onBegin: function () { me.summaryToggle.innerHTML = me.resources.pickwinnerview.hide_summary }
            });

            this.challengerSummaryOut = CoreFX.wipeOut({
                node: me.challengerPickWinnerSummaryNode,
                rate: 15
            });

            this.challengerSummaryIn = CoreFX.wipeIn({
                node: me.challengerPickWinnerSummaryNode,
                rate: 15
            });
        },

        _onCancelClick: function () {
            var me = this;
            this.kpiSummaryWidgets = new Array();
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
            this.kpiSummaryWidgets.push(textHelper.renderControlSummary(this.controlPickWinnerSummaryNode, this.controlConversionPercent));
            this.kpiSummaryWidgets.push(textHelper.renderChallengerSummary(this.challengerPickWinnerSummaryNode, this.challengerConversionPercent));
            textHelper.renderDescription(this.testDescription);
            textHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
            this._renderSignificance();

            ready(function () {
                pubThumb = document.getElementById("publishThumbnailpickwinner");
                draftThumb = document.getElementById("draftThumbnailpickwinner");
                if (me.context.customViewType == "marketing-testing/views/PickWinner") {
                    thumbnails._setThumbnail(pubThumb, me.context.data.publishPreviewUrl);
                    thumbnails._setThumbnail(draftThumb, me.context.data.draftPreviewUrl);
                };
                me._renderStatusIndicatorStyles();
                me._renderKpiMarkup("pw_conversionMarkup");
                for (x = 0; x < me.kpiSummaryWidgets.length; x++) {
                    me.kpiSummaryWidgets[x].startup();
                }
            });
            this._resetView();
        },

        _renderKpiMarkup: function (conversionMarkupId) {
            this.disablePickButtons(false);
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

        _clearKpiDescription: function (conversionMarkupElement) {
            if (conversionMarkupElement) {
                var contentPane = dojo.query('#pw_kpidescription > *');
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
            var me = this;
            this.disablePickButtons(true);
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.publishedVersionContentLink,
                testId: this.context.data.test.id
            }, { id: this.context.data.test.id }, { "options.incremental": false })  // Force a put
                    .then(function (testId) {
                        this.kpiSummaryWidgets = new Array();
                        var contextParameters = { uri: "epi.marketing.testing:///testid=" + testId + "/Archive" };
                        topic.publish("/epi/shell/context/request", contextParameters);
                    }).otherwise(function () {
                        alert("Error Processing Winner: Unable to process and save selected version");
                        console.log("Error occurred while processing winning content");
                        me.disablePickButtons(false);
                    });
        },

        _onVariantVersionClick: function () {
            var me = this;
            this.disablePickButtons(true);
            if (this.publishButtonClickCounter > 0) { return false; } // Use click counter to prevent double-click
            this.publishButtonClickCounter++; // Increment click count
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.draftVersionContentLink,
                testId: this.context.data.test.id
            }, { id: this.context.data.test.id }, { "options.incremental": false }) // Force a put
                .then(function (testId) {
                    this.kpiSummaryWidgets = new Array();
                    var contextParameters = { uri: "epi.marketing.testing:///testid=" + testId + "/Archive" };
                    topic.publish("/epi/shell/context/request", contextParameters);
                }).otherwise(function () {
                    alert("Error Processing Winner: Unable to process and save selected version");
                    console.log("Error occurred while processing winning content");
                    me.disablePickButtons(false);
                });
        },

        disablePickButtons: function (isDisabled) {
            this.controlPickButton.set("disabled", isDisabled);
            this.challengerPickButton.set("disabled", isDisabled);
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

        _toggleSummaries: function () {
            if (this.summaryToggle.innerHTML === this.resources.pickwinnerview.hide_summary) {
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