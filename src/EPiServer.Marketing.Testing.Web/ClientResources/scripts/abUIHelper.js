define([
"dojo/dom",
"dojox/charting/Chart",
"dojox/charting/plot2d/Pie",
"epi/i18n!marketing-testing/nls/abtesting",
"epi/datetime",
"epi/username",
"dojo/dom-class",


], function (dom, chart, pie, resources, datetime, username, domClass) {
    //"privates"
    var context;

    //used to cacluate the percentages for the control and challenger content.
    function getPercent(visitors, conversions) {
        if (conversions === 0) {
            return 0;
        }
        var percent = (visitors / conversions) * 100;
        return Math.round(percent);
    };

    return {
        resources: resources,

        publishedVariant: null,
        draftVariant: null,
        publishedPercent: null,
        draftPercent: null,

        //sets the helpers context value as well as initializes calculated variables
        initializeHelper: function (testContext) {
            context = testContext;

            if (context.data.test.variants[0].itemVersion ===
                context.data.publishedVersionContentLink.split('_')[0]) {
                this.publishedVariant = context.data.test.variants[0];
                this.draftVariant = context.data.test.variants[1];
            } else {
                this.publishedVariant = context.data.test.variants[1];
                this.draftVariant = context.data.test.variants[0];
            }

            this.publishedPercent = getPercent(this.publishedVariant.conversions, this.publishedVariant.views);
            this.draftPercent = getPercent(this.draftVariant.conversions, this.draftVariant.views);

        },

        //sets text content of provided node to the context test title
        renderTitle: function (titleNode) {
            titleNode.textContent = context.data.test.title;
        },

        //sets text content of status and started notes to a string formatted based on the
        //state of the context test
        renderTestStatus: function (testStatusNode, testStartedNode) {
            if (context.data.test.state === 0) {
                testStatusNode.textContent = resources.detailsview.test_status_not_started;
                testStartedNode.textContent = resources.detailsview.test_scheduled +
                    datetime.toUserFriendlyString(context.data.test.startDate);
            } else if (context.data.test.state === 1) {
                testStatusNode
                    .textContent = resources.detailsview.test_status_running + context.data.daysRemaining + " " + resources.detailsview.days_remaining;
                testStartedNode.textContent = resources.detailsview.started +
                    datetime.toUserFriendlyString(context.data.test.startDate) +
                    " " +
                    resources.detailsview.by +
                    " " +
                    username.toUserFriendlyString(context.data.test.owner);

            } else {
                testStatusNode.textContent = resources.detailsview.test_status_completed;
                testStartedNode.textContent = "";
            }
        },

        //sets text content of provided node to the context test duration
        renderTestDuration: function (testDurationNode) {
            testDurationNode.textContent = context.data.daysElapsed;
        },

        //sets text content of provided node to the context test time remaining
        renderTestRemaining: function (testRemainingNode) {
            testRemainingNode.textContent = context.data.daysRemaining;
        },

        //sets text content of provided node to the context confidence level
        renderConfidence: function (confidenceNode) {
            confidenceNode.textContent = context.data.test.confidenceLevel + "%";
        },

        //sets text content of provided nodes to the published content publishedy by and date published values
        renderPublishedInfo: function (publishedByNode, datePublishedNode) {
            publishedByNode.textContent = username.toUserFriendlyString(context.data.publishedVersionPublishedBy);
            datePublishedNode.textContent = datetime.toUserFriendlyString(context.data.publishedVersionPublishedDate);
        },

        //sets text content of provided nodes to the draft content changed by and date changed values
        renderDraftInfo: function (changedByNode, dateChangedNode) {
            changedByNode.textContent = username.toUserFriendlyString(context.data.draftVersionChangedBy);
            dateChangedNode.textContent = datetime.toUserFriendlyString(context.data.draftVersionChangedDate);
        },

        //sets text content of provided nodes to the published variant conversions, views and conversion percent
        renderPublishedViewsAndConversions: function (publishedConversionsNode, publishedViewsNode, publishedConversionPercentNode) {
            publishedConversionsNode.textContent = this.publishedVariant.conversions;
            publishedViewsNode.textContent = this.publishedVariant.views;
            publishedConversionPercentNode.textContent = this.publishedPercent + "%";
        },

        //sets text content of provided nodes to the draft variant conversions, views and conversion percent
        renderDraftViewsAndConversions: function (challengerConversionsNode, challengerViewsNode, challengerConversionPercentNode) {
            challengerConversionsNode.textContent = this.draftVariant.conversions;
            challengerViewsNode.textContent = this.draftVariant.views;
            challengerConversionPercentNode.textContent = this.draftPercent + "%";
        },

        //sets text content of provided node to a formatted version of the context test description
        renderDescription: function (testDescriptionNode) {
            //Test description, visitor percentage and total participants
            if (context.data.test.description) {
                testDescriptionNode.textContent = "\"" +
                    context.data.test.description +
                    "\" - " + username.toUserFriendlyString(context.data.test.owner);
            } else testDescriptionNode.textContent = "";
        },

        //sets the styling of the control and challnger sections based on their conversion percentages
        renderStatusIndicatorStyles: function (controlStatusIconNode, challengerStatusIcondNode, controlWrapperNode, challengerWrapperNode) {
            var me = this;
            if (context.data.test.state < 2) {
                me.statusIndicatorClass = "leadingContent";
            }
            else { me.statusIndicatorClass = "winningContent"; }

            if (this.publishedPercent > this.draftPercent) {
                domClass.replace(controlStatusIconNode, me.statusIndicatorClass);
                domClass.replace(challengerStatusIcondNode, "noIndicator");
                domClass.replace(controlWrapperNode, "cardWrapper 2column controlLeaderBody");
                domClass.replace(challengerWrapperNode, "cardWrapper 2column challengerDefaultBody");
            }
            else if (this.publishedPercent < this.draftPercent) {
                domClass.replace(controlStatusIconNode, "noIndicator");
                domClass.replace(challengerStatusIcondNode, me.statusIndicatorClass);
                domClass.replace(controlWrapperNode, "cardWrapper 2column controlTrailingBody");
                domClass.replace(challengerWrapperNode, "cardWrapper 2column challengerLeaderBody");
            }
            else {
                domClass.replace(controlStatusIconNode, "noIndicator");
                domClass.replace(challengerStatusIcondNode, "noIndicator");
                domClass.replace(controlWrapperNode, "cardWrapper 2column controlDefaultBody");
                domClass.replace(challengerWrapperNode, "cardWrapper 2column challengerDefaultBody");
            }
        },

        //sets text content of provided nodes to the context participation percentage and total participation values
        renderVisitorStats: function (participationPercentageNode, totalParticipantsNode) {
            participationPercentageNode.textContent = context.data.visitorPercentage;
            totalParticipantsNode.textContent = context.data.totalParticipantCount;
        },

        //sets text content of provided link node to the context conversion link values
        renderConversion: function (contentLinkAnchorNode) {
            contentLinkAnchorNode.href = context.data.conversionLink;
            contentLinkAnchorNode.textContent = context.data.conversionContentName;
        },

        //Checks for an available node and attaches a pie chart widget
        //show based on a single value of a 100%
        displayPieChart: function (node, data) {
            if (dom.byId(node)) {
                dom.byId(node).innerHTML = "";

                var chartNode = dom.byId(node);
                var pieChart = new chart(chartNode);

                var chartData = [
                    {
                        x: 1,
                        y: 100 - data,
                        fill: "#edebe9"
                    }, {
                        x: 1,
                        y: data,
                        fill: "#86c740"
                    }
                ];

                pieChart.addPlot("default",
                {
                    type: "Pie",
                    labels: false,
                    radius: 50
                });
                pieChart.addSeries("", chartData, { stroke: { width: 0 } });
                pieChart.render();
            }
        }
    };
});