define([
"dojo/dom",
"dijit/registry",
"epi/datetime",
"epi/username",
"dojo/dom-class",
"marketing-testing/widgets/KpiSummaryWidget",
"marketing-testing/widgets/KpiSummariesWidget",
"marketing-testing/widgets/ConversionPercentTemplate"
],

function (dom, registry, datetime, username, domClass, KpiSummaryWidget, KpiSummariesWidget, ConversionPercentTemplate) {
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

            this.publishedVariant = this._findInArray(context.data.test.variants, "isPublished", true);
            this.draftVariant = this._findInArray(context.data.test.variants, "isPublished", false);

            this.publishedPercent = getPercent(this.publishedVariant.conversions, this.publishedVariant.views);
            this.draftPercent = getPercent(this.draftVariant.conversions, this.draftVariant.views);
            if (stringResources) {
                resources = stringResources;
            }

            //support for unit tests
            if (mModules) {
                username = mModules.username;
                domClass = mModules.domClass;
                ConversionPercentTemplate = mModules.conversionpercenttemplate;
                KpiSummariesWidget = mModules.kpisummarieswidget;
                KpiSummaryWidget = mModules.kpisummarywidget;
            };
        },

        // replacement for .find to support IE
        //arrayObj = array to search, property = property to seach on, value = value to match
        _findInArray: function (arrayObj, property, value) {
            for (var i = 0; i < arrayObj.length; i++) {
                if (arrayObj[i][property] === value) {
                    return arrayObj[i];
                }
            }
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
                    dojo.forEach(registry.findWidgets(controlWidget)), function (w) {
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

        //destroys any percentage widgets loaded into the view
        _removePercentageWidgets: function (percentageWidgetNode) {
            var me = this;
            if (percentageWidgetNode) {
                var percentageWidget = dojo.query(percentageWidgetNode);
                if (percentageWidget) {
                    dojo.forEach(registry.findWidgets(percentageWidget)), function (w) {
                        var widgetToRemove = me.kpiSummaryWidgets.indexOf(w);
                        if (widgetToRemove) {
                            me.kpiSummaryWidgets.splice(widgetToRemove, 1);
                        }
                        w.destroyRecursive();
                    };
                    percentageWidgetNode.innerHTML = "";
                }
            }
        },

        //renders test summary for control (published) content
        renderControlSummary: function (summaryNode, controlPercentageNode) {
            this._removeSummaryWidgets(summaryNode)
            this._removePercentageWidgets(controlPercentageNode);
            var summaryWidget;

            if (context.data.test.kpiInstances.length > 1) {
                summaryWidget = this._renderControlSummaries(summaryNode);
                new ConversionPercentTemplate({
                    conversionPercent: this.publishedPercent,
                    views: this.publishedVariant.views,
                    isLeader: eval(this.publishedPercent > this.draftPercent)
                }).placeAt(controlPercentageNode);
            }
            else {
                var kpiResultType = context.data.kpiResultType;
                var hasChart = true;
                var kpiConversions;
                if (kpiResultType === "KpiFinancialResult") {
                    hasChart = false;
                    kpiConversions = this.publishedVariant.keyFinancialResults.length;
                    kpiConversionRate = context.data.publishedVersionValuesAverage;
                } else if (kpiResultType === "KpiValueResult") {
                    hasChart = false;
                    kpiConversions = this.publishedVariant.keyValueResults.length;
                    kpiConversionRate = context.data.publishedVersionValuesAverage;
                } else {
                    hasChart = true;
                    kpiConversions = this.publishedVariant.conversions;
                    kpiConversionRate = this.publishedPercent;
                };

                summaryWidget = new KpiSummaryWidget({
                    displayChart: hasChart,
                    views: this.publishedVariant.views,
                    conversions: kpiConversions,
                    conversionRate: kpiConversionRate,
                    isLeader: eval(this.publishedPercent > this.draftPercent)
                });
            }
            summaryWidget.placeAt(summaryNode);
            return summaryWidget;
        },

        //renders test summary for challenger (draft) content.
        renderChallengerSummary: function (summaryNode, challengerPercentageNode) {
            this._removeSummaryWidgets(summaryNode)
            this._removePercentageWidgets(challengerPercentageNode);

            var summaryWidget;

            if (context.data.test.kpiInstances.length > 1) {
                summaryWidget = this._renderChallengerSummaries(summaryNode);
                new ConversionPercentTemplate({
                    conversionPercent: this.draftPercent,
                    views: this.draftVariant.views,
                    isLeader: eval(this.draftPercent > this.publishedPercent)
                }).placeAt(challengerPercentageNode);
            }
            else {
                var kpiResultType = context.data.kpiResultType;
                var hasChart = true;
                var kpiConversions;
                if (kpiResultType === "KpiFinancialResult") {
                    hasChart = false;
                    kpiConversions = this.draftVariant.keyFinancialResults.length;
                    kpiConversionRate = context.data.draftVersionValuesAverage;
                } else if (kpiResultType === "KpiValueResult") {
                    hasChart = false;
                    kpiConversions = this.draftVariant.keyValueResults.length;
                    kpiConversionRate = context.data.draftVersionValuesAverage;
                } else {
                    hasChart = true;
                    kpiConversions = this.draftVariant.conversions;
                    kpiConversionRate = this.draftPercent;
                };

                summaryWidget = new KpiSummaryWidget({
                    displayChart: hasChart,
                    views: this.draftVariant.views,
                    conversions: kpiConversions,
                    conversionRate: kpiConversionRate,
                    isLeader: eval(this.draftPercent > this.publishedPercent)
                });

            }
            summaryWidget.placeAt(summaryNode);
            return summaryWidget;
        },

        //renders multiple kpi summary for control (published) content
        _renderControlSummaries: function () {
            var kpiInstances = context.data.test.kpiInstances;
            var kpiResults = new Array();
            for (var x = 0; x < kpiInstances.length; x++) {
                var kpiSummary = this._findInArray(this.publishedVariant.keyConversionResults, "kpiId", kpiInstances[x].id);
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

        //renders multiple kpi summary for challenger (draft) content
        _renderChallengerSummaries: function () {
            var kpiInstances = context.data.test.kpiInstances;
            var kpiResults = new Array();
            for (var x = 0; x < kpiInstances.length; x++) {
                var kpiSummary = this._findInArray(this.draftVariant.keyConversionResults, "kpiId", kpiInstances[x].id);
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

        //sets and renders duration progress bar
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