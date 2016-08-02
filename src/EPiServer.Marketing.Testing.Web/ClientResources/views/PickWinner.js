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
 "dojo/text!marketing-testing/views/PickWinner.html",
 "epi/i18n!marketing-testing/nls/abtesting",
 "epi/datetime",
 "epi/username",
 "dojo/dom-class",
 "dojox/charting/Chart",
 "dojox/charting/plot2d/Pie",
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
    chart,
    pie

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
            this._renderData();
            this._renderChartData();

        },

        startup: function () {
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            me._renderData();
            me._renderChartData();
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var publishedVariant, draftVariant;

            //Header and Test Start Information
            this.pageName.textContent = this.context.data.test.title;

            if (this.context.data.test.state === 0) {
                this.headerStateAndElapsedText.textContent = resources.detailsview.test_status_not_started;
                this.headerStartedText.textContent = resources.detailsview.test_scheduled +
                    datetime.toUserFriendlyString(this.context.data.test.startDate);

            } else if (this.context.data.test.state === 1) {
                this.headerStateAndElapsedText
                    .textContent = resources.detailsview.test_status_running + this.context.data.daysRemaining + " " + resources.detailsview.days_remaining;
                this.headerStartedText.textContent = resources.detailsview.started +
                    datetime.toUserFriendlyString(this.context.data.test.startDate) +
                    " " +
                    resources.detailsview.by +
                    " " +
                    username.toUserFriendlyString(this.context.data.test.owner);

            } else if (this.contexxt.data.test.state === 3) {
                this.headerStateAndElapsedText.textContent = resources.detailsview.test_status_completed;
                this.headerStartedText.textContent = "";
            }

            this.testDuration.textContent = this.context.data.daysElapsed;
            this.timeRemaining.textContent = this.context.data.daysRemaining;
            this.selectedConfidenceLevel.textContent = this.context.data.test.confidenceLevel + "%";

            //Published version data
            this.publishedBy.textContent = username.toUserFriendlyString(this.context.data.publishedVersionPublishedBy);
            this.datePublished.textContent = datetime.toUserFriendlyString(this.context.data.publishedVersionPublishedDate);

            //Draft version data
            this.savedBy.textContent = username.toUserFriendlyString(this.context.data.draftVersionChangedBy);
            this.dateSaved.textContent = datetime.toUserFriendlyString(this.context.data.draftVersionChangedDate);

            //Set the correct corresponding variant data
            if (this.context.data.test.variants[0].itemVersion === this.context.data.publishedVersionContentLink.split('_')[0]) {
                publishedVariant = this.context.data.test.variants[0];
                draftVariant = this.context.data.test.variants[1];
            } else {
                publishedVariant = this.context.data.test.variants[1];
                draftVariant = this.context.data.test.variants[0];
            }

            var publishedPercent = getPercent(publishedVariant.conversions, publishedVariant.views);
            var variantPercent = getPercent(draftVariant.conversions, draftVariant.views);

            //Published version views/conversions and meter
            this.publishedVersionConversions.textContent = publishedVariant.conversions;
            this.publishedVersionViews.textContent = publishedVariant.views;
            this.publishedVersionPercentage.textContent = publishedPercent + "%";


            //Draft version views/conversions and meter
            this.variantConversions.textContent = draftVariant.conversions;
            this.variantViews.textContent = draftVariant.views;
            this.variantPercentage.textContent = variantPercent + "%";

            //Test description, visitor percentage and total participants
            this.testDescription.textContent = "\"" +
                this.context.data.test.description +
                "\" - " + username.toUserFriendlyString(this.context.data.test.owner);

            this.visitorPercentageText.textContent = this.context.data.visitorPercentage;
            this.totalParticipantsText.textContent = this.context.data.totalParticipantCount;

            this.contentLinkAnchor.href = this.context.data.conversionLink;
            this.contentLinkAnchor.textContent = this.context.data.conversionContentName;

            //Set Test Result Status Icons and styles
            //Icons are to show on content which is ahead in conversoins
            //Styles change size and color of container to accomodate for border
            //keeping things aligned properly.
            //Styles and icons adjust based on A > B, A < B, A = B, and 0 and 
            //whether test is active or complete.
            var statusIndicatorClass = "noIndicator";

            if (this.context.data.test.state === 1) {
                statusIndicatorClass = "leadingContent";
            }
            else if (this.context.data.test.state > 2) {
                statusIndicatorClass = "winningContent";
            }

            if (publishedPercent > variantPercent) {
                domClass.replace(this.publishedStatusIcon, statusIndicatorClass);
                domClass.replace(this.variantStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, "cardWrapper 2column controlLeaderBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column challengerDefaultBody");
            }
            else if (publishedPercent < variantPercent) {
                domClass.replace(this.publishedStatusIcon, "noIndicator");
                domClass.replace(this.variantStatusIcon, statusIndicatorClass);
                domClass.replace(this.controlWrapper, "cardWrapper 2column controlTrailingBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column challengerLeaderBody");
            }
            else {
                domClass.replace(this.publishedStatusIcon, "noIndicator");
                domClass.replace(this.variantStatusIcon, "noIndicator");
                domClass.replace(this.controlWrapper, "cardWrapper 2column controlDefaultBody");
                domClass.replace(this.challengerWrapper, "cardWrapper 2column challengerDefaultBody");

            }
        },

        _onPublishedVersionClick: function () {
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.publishedVersionContentLink,
                testId: this.context.data.test.id
            }, { id: this.context.data.test.id }, { "options.incremental": false })  // Force a put
                    .then(function (data) {
                        var contextParameters = { uri: "epi.cms.contentdata:///" + data };
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
                .then(function (data) {
                    var contextParameters = { uri: "epi.cms.contentdata:///" + data };
                    topic.publish("/epi/shell/context/request", contextParameters);
                }).otherwise(function () {
                    alert("Error Processing Winner: Unable to process and save selected version");
                    console.log("Error occurred while processing winning content");
                });
        },

        _renderChartData() {
            dom.byId("controlPieChart").innerHTML = "";
            dom.byId("challengerPieChart").innerHTML = "";
            var controlPercentage = this.publishedVersionPercentage.textContent
                .substr(0, this.publishedVersionPercentage.textContent.length - 1);
            var challengerPercentage = this.variantPercentage.textContent
                .substr(0, this.variantPercentage.textContent.length - 1);
            this._displayPieChart("controlPieChart", Number(controlPercentage));
            this._displayPieChart("challengerPieChart", Number(challengerPercentage));
        },

        _displayPieChart(node, data) {
            var chartNode = dom.byId(node);
            var pieChart = new chart(chartNode);

            var chartData = [{
                x: 1,
                y: 100 - data,
                fill: "#edebe9"
            }, {
                x: 1,
                y: data,
                fill: "#86c740"
            }];

            pieChart.addPlot("default", {
                type: "Pie",
                labels: false,
                radius: 50
            });
            pieChart.addSeries("", chartData, { stroke: { width: 0 } });
            pieChart.render();
        }
    });

    function getPercent(visitors, conversions) {
        if (conversions === 0) {
            return 0;
        }
        var percent = (visitors / conversions) * 100;
        return Math.round(percent);
    }
});