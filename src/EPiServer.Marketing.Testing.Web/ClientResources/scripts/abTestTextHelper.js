define([
"dojo/dom",
"dojox/charting/Chart",
"dojox/charting/plot2d/Pie",
"epi/datetime",
"epi/username",
"dojo/dom-class",
"marketing-testing/widgets/KpiSummaryWidget",
"marketing-testing/widgets/KpiSummariesWidget"

],

function (dom, chart, pie, datetime, username, domClass, KpiSummaryWidget, KpiSummariesWidget) {
    //"privates"
    var context, resources;

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
                testRemainingNode.textContent = resources.test_status_not_started;
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

        //creates and places appropriate summary widgets
        _removeSummaryWidgets: function (summaryNode) {
            var me = this;
            if (summaryNode) {
                var controlWidget = dojo.query(summaryNode);
                if (controlWidget) {
                    dojo.forEach(dijit.findWidgets(controlWidget)), function (w) {
                        var widgetToRemove = me.kpiSummaryWidgets.indexOf(w);
                        if (widgetToRemove) {
                            me.kpiSummaryWidgets.splice(widgetToRemove, 1);
                        }
                        w.destroyRecursive();
                    };
                    summaryNode.innerHTML = "";
                }
            }
        },

        renderControlSummary: function (summaryNode) {
            this._removeSummaryWidgets(summaryNode)
            var summaryWidget;

            if (context.data.test.kpiInstances.length > 1) {
                summaryWidget = this._renderControlSummaries(summaryNode)
            }
            else {
                var kpiResultType = context.data.kpiResultType;
                if (kpiResultType === "KpiFinancialResult") {
                    summaryWidget = new KpiSummaryWidget({
                        displayChart: false,
                        views: this.publishedVariant.views,
                        conversions: this.publishedVariant.keyFinancialResults.length,
                        conversionRate: context.data.publishedVersionFinancialsAverage,
                        isLeader: eval(this.publishedPercent > this.draftPercent)
                    });
                }
                else {
                    summaryWidget = new KpiSummaryWidget({
                        views: this.publishedVariant.views,
                        conversions: this.publishedVariant.conversions,
                        conversionRate: this.publishedPercent,
                        isLeader: eval(this.publishedPercent > this.draftPercent)

                    });
                }
            }
            summaryWidget.placeAt(summaryNode);
            return summaryWidget;
        },

        renderChallengerSummary: function (summaryNode) {
            this._removeSummaryWidgets(summaryNode)
            var summaryWidget;

            if (context.data.test.kpiInstances.length > 1) {
                summaryWidget = this._renderChallengerSummaries(summaryNode)
            }
            else {
                var kpiResultType = context.data.kpiResultType;
                if (kpiResultType === "KpiFinancialResult") {
                    summaryWidget = new KpiSummaryWidget({
                        displayChart: false,
                        views: this.draftVariant.views,
                        conversions: this.draftVariant.keyFinancialResults.length,
                        conversionRate: context.data.draftVersionFinancialsAverage,
                        isLeader: eval(this.draftPercent > this.publishedPercent)
                    });
                }
                else {
                    summaryWidget = new KpiSummaryWidget({
                        views: this.draftVariant.views,
                        conversions: this.draftVariant.conversions,
                        conversionRate: this.draftPercent,
                        isLeader: eval(this.draftPercent > this.publishedPercent)
                    });
                }
            }
            summaryWidget.placeAt(summaryNode);
            return summaryWidget;
        },

        _getKpiSummary: function (id, arrayObj) {
            for (var i = 0; i < arrayObj.length; i++) {
                if (arrayObj[i].kpiId === id) {
                    return arrayObj[i];
                }
            }
        },

        _renderControlSummaries: function () {
            var kpiInstances = context.data.test.kpiInstances;
            var kpiResults = new Array();
            for (var x = 0; x < kpiInstances.length; x++) {
                var kpiSummary = this._getKpiSummary(kpiInstances[x].id, this.publishedVariant.keyConversionResults);
                var kpiResult = {
                    markup: kpiInstances[x].uiReadOnlyMarkup,
                    conversions: kpiSummary.conversions,
                    weight: kpiSummary.selectedWeight,
                    performance: kpiSummary.performance
                }
                kpiResults.push(kpiResult);
            }
            var summaries = new KpiSummariesWidget({
                kpis: kpiResults,
            })
            return summaries;
        },

        _renderChallengerSummaries: function () {
            var kpiInstances = context.data.test.kpiInstances;
            var kpiResults = new Array();
            for (var x = 0; x < kpiInstances.length; x++) {
                var kpiSummary = this._getKpiSummary(kpiInstances[x].id, this.draftVariant.keyConversionResults);
                var kpiResult = {
                    markup: kpiInstances[x].uiReadOnlyMarkup,
                    conversions: kpiSummary.conversions,
                    weight: kpiSummary.selectedWeight,
                    performance: kpiSummary.performance
                }
                kpiResults.push(kpiResult);
            }
            var summaries = new KpiSummariesWidget({
                kpis: kpiResults,
            })
            return summaries;
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

        renderDurationProgress: function (durationProgressIndicatorNode) {
            var totalTestDuration = Number(context.data.daysElapsed) + Number(context.data.daysRemaining);
            durationProgressIndicatorNode.set({ maximum: totalTestDuration });

            if (context.data.test.state === 0) {
                durationProgressIndicatorNode.set({ value: 0 });
            } else {
                durationProgressIndicatorNode.set({ value: context.data.daysElapsed });
            }
        },
    };
});