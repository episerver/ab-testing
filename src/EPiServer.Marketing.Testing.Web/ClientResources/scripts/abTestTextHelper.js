define([
"dojo/dom",
"dojox/charting/Chart",
"dojox/charting/plot2d/Pie",
"epi/datetime",
"epi/username",
"dojo/dom-class"

],

function (dom, chart, pie, datetime, userModule, dojoDomClass) {
    //"privates"
    var context, resources, username = userModule, domClass = dojoDomClass;

    //used to cacluate the percentages for the control and challenger content.
    function getPercent(visitors, conversions) {
        if (conversions === 0) {
            return 0;
        }
        var percent = (visitors / conversions) * 100;
        return Math.round(percent);
    };

    return {

        publishedVariant: null,
        draftVariant: null,
        publishedPercent: null,
        draftPercent: null,

        //sets the helpers context value as well as initializes calculated variables
        initializeHelper: function (testContext, stringResources, mModules) {
            context = testContext;

            this.publishedVariant = context.data.test.variants.find(function (obj) { return obj.isPublished });
            this.draftVariant = context.data.test.variants.find(function (obj) { return !obj.isPublished });

            this.publishedPercent = getPercent(this.publishedVariant.conversions, this.publishedVariant.views);
            this.draftPercent = getPercent(this.draftVariant.conversions, this.draftVariant.views);
            if (stringResources) {
                resources = stringResources;
            }

            if (mModules) {
                username = mModules.username;
                domClass = mModules.domClass;
            };

        },

        //sets text content of provided node to the context test title
        renderTitle: function (titleNode) {
            titleNode.textContent = context.data.test.title;
        },

        //sets text content of status and started notes to a string formatted based on the
        //state of the context test
        renderTestStatus: function (testStatusNode, testStartedNode) {
            if (context.data.test.state === 0) {
                testStatusNode.textContent = resources.test_status_not_started;
                testStartedNode.textContent = resources.test_scheduled +
                    datetime.toUserFriendlyString(context.data.test.startDate);
            } else if (context.data.test.state === 1) {
                testStatusNode
                    .textContent = resources.test_status_running + context.data.daysRemaining + " " + resources.days_remaining;
                testStartedNode.textContent = resources.started +
                    datetime.toUserFriendlyString(context.data.test.startDate) +
                    " " +
                    resources.by +
                    " " +
                    username.toUserFriendlyString(context.data.test.owner);

            } else {
                testStatusNode.textContent = resources.test_status_completed;
                testStartedNode.textContent = "";
            }
        },

        //sets text content of provided node to the context test duration
        renderTestDuration: function (testDurationNode) {
            if (Number(context.data.test.state) === 0) {
                testDurationNode.textContent = 0;
            } else {
                testDurationNode.textContent = context.data.daysElapsed;
            }
        },

        //sets text content of provided node to the context test time remaining
        renderTestRemaining: function (testRemainingNode, testRemainingTextNode) {
            if (Number(context.data.test.state) === 0) {
                testRemainingNode.textContent = resources.test_not_started_text;
                testRemainingTextNode.textContent = "";
            } else if (Number(context.data.test.state) > 1) {
                testRemainingNode.textContent = "";
                testRemainingTextNode.textContent = resources.test_duration_completed;
            }
            else {
                testRemainingNode.textContent = context.data.daysRemaining;
                testRemainingTextNode.textContent = resources.days_remaining;
            }

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
            } else testDescriptionNode.textContent = resources.goal_not_specified;
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

        renderSignificance: function (pickAWinnerMessageNode) {
            if (context.data.test.state < 2) {
                pickAWinnerMessageNode.innerHTML = resources.early_pick_winner_message;
            } else if (context.data.test.state === 2) {
                if (context.data.test.isSignificant) {
                    pickAWinnerMessageNode.innerHTML = resources.result_is_significant;
                } else {
                    pickAWinnerMessageNode.innerHTML = resources.result_is_not_significant;
                }
            }
        },

        renderDurationProgress: function (durationProgressIndicatorNode) {
            var totalTestDuration = Number(context.data.daysElapsed) + Number(context.data.daysRemaining);
            durationProgressIndicatorNode.set({ maximum: totalTestDuration });
            durationProgressIndicatorNode.set({ value: context.data.daysElapsed });
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