define([
    'dojo/dom-construct',
        'marketing-testing/scripts/abTestTextHelper',
        'epi/_Module'
],
    function (DomConstruct, abTestTextHelper, Module) {
        describe("abTestTextHelper - Details View",
            function () {
                var mockStringResources = {
                    test_status_running: "Test is running, ",
                    days_remaining: "day(s) remaining.",
                    started: "started ",
                    test_status_completed: "Test completed, now go on and pick a winner...",
                    test_status_not_started: "Test has not yet started. ",
                    test_scheduled: "It is scheduled to begin ",
                    by: "by",
                    result_is_not_significant: "The results of this test are NOT significant.",
                    result_is_significant: "The results of this test are significant.",
                    early_pick_winner_message:
                        "This test has not been completed, but you may pick a winner. Picking a winner now will end the test and publish the content chosen.",
                    test_duration_completed: "Completed after full duration",
                    goal_not_specified: "A goal has not been specified."
                };
                var defaultTextContent = "Default Text",
                    context = {
                        data: {
                            daysElapsed: 4,
                            daysRemaining: 26,
                            publishedVersionPublishedBy: "MockPublishedUser",
                            publishedVersionPublishedDate: "2016-05-01 20:00:00.000",
                            publishedVersionContentLink: "3",
                            draftVersionChangedBy: "MockChangedByUser",
                            draftVersionChangedDate: "2016-05-02 21:00:00.000",
                            visitorPercentage: 50,
                            totalParticipantCount: 200,
                            conversionLink: "testLink",
                            conversionContentName: "conversion content",
                            test: {
                                title: "Unit Test Title",
                                startDate: "2016-08-03 15:27:50.000",
                                endDate: "2016-09-02 15:27:50.000",
                                state: 0,
                                variants: [],
                                confidenceLevel: 98,
                                owner: "MockTestOwner",
                                description: "Mock Test Description",
                                isSignificant: true
                            }
                        }
                    },
                    mockDependencies = {
                        username: {
                            toUserFriendlyString: function (name) {
                                return name;
                            }
                        },
                        domClass: {
                            replace: function (oldClass, newClass) {
                                oldClass.mockClass = newClass;
                            }
                        }
                    };
                context.data.test.variants.push({ itemVersion: "3", isPublished: true, conversions: 10, views: 100 });
                context.data.test.variants.push({ itemVersion: "150", isPublished: false, conversions: 150, views: 200 });

                it("Correctly initializes properties based on given context",
                    function () {
                        abTestTextHelper.initializeHelper(context);
                        expect(abTestTextHelper.publishedVariant.itemVersion).to.equal("3");
                        expect(abTestTextHelper.publishedVariant.conversions).to.equal(10);
                        expect(abTestTextHelper.publishedVariant.views).to.equal(100);
                        expect(abTestTextHelper.draftVariant.itemVersion).to.equal("150");
                        expect(abTestTextHelper.draftVariant.conversions).to.equal(150);
                        expect(abTestTextHelper.draftVariant.views).to.equal(200);
                        expect(abTestTextHelper.publishedPercent).to.equal(10);
                        expect(abTestTextHelper.draftPercent).to.equal(75)
                    });

                it("Searches and finds the correct object based on supplied property and value",
                    function () {
                        var mockArray = new Array();
                        var mockObject = {
                            value1: "A",
                            value2: "B",
                            value3: "C"
                        }
                        mockArray.push(mockObject);
                        var mockObject = {
                            value1: "D",
                            value2: "E",
                            value3: "F"
                        }
                        mockArray.push(mockObject);
                        var returnedObject = abTestTextHelper._findInArray(mockArray, "value2", "F");

                        expect(returnedObject).to.equal(mockObject[1]);
                    });

                it("Correctly sets the textContent of the provided title element",
                    function () {
                        var mockTitleElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderTitle(mockTitleElement);

                        expect(mockTitleElement.textContent).to.equal("Unit Test Title");
                    });

                it("Renders correct status and messaging for a test which is not started (state = 0)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context, mockStringResources);
                        abTestTextHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test has not yet started. ");
                        expect(mockStartedElement.textContent).to.equal("It is scheduled to begin 8/3/16, 3:27 PM")

                    });

                it("Renders correct status and messaging for a test which is active (state = 1)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };
                        context.data.test.state = 1;

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test is running, 26 day(s) remaining.");
                        expect(mockStartedElement.textContent).to.equal("started 8/3/16, 3:27 PM by MockTestOwner");

                    });

                it("Renders correct status and messaging for a test which is done (state = 2)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };
                        context.data.test.state = 2;

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test completed, now go on and pick a winner...");
                        expect(mockStartedElement.textContent).to.equal("");

                    });

                it("Renders correct daysElapsed based on the context provided",
                    function () {
                        var mockDaysElapsedElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderTestDuration(mockDaysElapsedElement);

                        expect(mockDaysElapsedElement.textContent).to.equal(4);
                    });

                it("Renders correct timeRemaining for tests not started (state = 0)",
                    function () {
                        context.data.test.state = 0;

                        var mockDaysRemainingElement = { textContent: defaultTextContent };
                        var mockDaysRemainingText = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderTestRemaining(mockDaysRemainingElement, mockDaysRemainingText);

                        expect(mockDaysRemainingElement.textContent).to.equal("Test has not yet started. ");
                        expect(mockDaysRemainingText.textContent).to.equal("");
                    });

                it("Renders correct timeRemaining for an active test (state = 1)",
                   function () {
                       context.data.test.state = 1;

                       var mockDaysRemainingElement = { textContent: defaultTextContent };
                       var mockDaysRemainingText = { textContent: defaultTextContent };

                       abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                       abTestTextHelper.renderTestRemaining(mockDaysRemainingElement, mockDaysRemainingText);

                       expect(mockDaysRemainingElement.textContent).to.equal(26);
                       expect(mockDaysRemainingText.textContent).to.equal("day(s) remaining.");
                   });

                it("Renders correct timeRemaining for completed tests (state = 2)",
                   function () {
                       context.data.test.state = 2;

                       var mockDaysRemainingElement = { textContent: defaultTextContent };
                       var mockDaysRemainingText = { textContent: defaultTextContent };

                       abTestTextHelper.initializeHelper(context);
                       abTestTextHelper.renderTestRemaining(mockDaysRemainingElement, mockDaysRemainingText);

                       expect(mockDaysRemainingElement.textContent).to.equal("");
                       expect(mockDaysRemainingText.textContent).to.equal("Completed after full duration");
                   });

                it("Renders correct confidence based on the context provided",
                    function () {
                        var mockConfidenceLevelElement = { defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderConfidence(mockConfidenceLevelElement);

                        expect(mockConfidenceLevelElement.textContent).to.equal("98%");
                    });

                it("Renders correct published(control) content information based on the context provided",
                    function () {
                        var mockPublishedElement = { textContent: defaultTextContent },
                            mockDatePublishedElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderPublishedInfo(mockPublishedElement, mockDatePublishedElement);

                        expect(mockPublishedElement.textContent).to.equal("MockPublishedUser");
                        expect(mockDatePublishedElement.textContent).to.equal("5/1/16, 8:00 PM");
                    });

                it("Renders correct draft(challenger) content information based on the context provided",
                    function () {
                        var mockChangedByElement = { textContent: defaultTextContent },
                            mockdateChangedElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderDraftInfo(mockChangedByElement, mockdateChangedElement);

                        expect(mockChangedByElement.textContent).to.equal("MockChangedByUser");
                        expect(mockdateChangedElement.textContent).to.equal("5/2/16, 9:00 PM");
                    });

                it("Renders correct published(control) variant views, conversions and conversion percent based on the context provided",
                    function () {
                        context.data.test.kpiInstances = [{ id: "kpi1" },
                                                          { id: "kpi2" },
                                                          { id: "kpi3" }];

                        context.data.test.variants[0].keyConversionResults = new Array();
                        context.data.test.variants[0].keyConversionResults.push({ kpiId: "kpi1", conversions: 10, views: 5 });
                        context.data.test.variants[0].keyConversionResults.push({ kpiId: "kpi2", conversions: 20, views: 6 });
                        context.data.test.variants[0].keyConversionResults.push({ kpiId: "kpi3", conversions: 30, views: 7 });

                        var summaryNode = DomConstruct.toDom("<div id='summaryNode'></div>");
                        var controlPercentageNode = DomConstruct.toDom("<div id='controlPercentageNode'></div>");
                        var widget = abTestTextHelper.renderControlSummary(summaryNode, controlPercentageNode);
                        expect("x").to.equal("y");
                    });

                it("Renders correct draft(challenger) variant views, conversions and conversion percent based on the context provided",
                    function () {
                        expect("This needs to be ").to.equal("refactored")
                    });

                it("Renders a properly formatted description when description is not null",
                    function () {
                        var mockDescriptionNode = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderDescription(mockDescriptionNode);

                        expect(mockDescriptionNode.textContent).to.equal("\"Mock Test Description\" - MockTestOwner");
                    });

                it("Renders description as an empty string when description is null",
                    function () {
                        var mockDescriptionNode = { textContent: defaultTextContent };
                        context.data.test.description = null;

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderDescription(mockDescriptionNode);

                        expect(mockDescriptionNode.textContent).to.equal("A goal has not been specified.");
                    });

                it("Renders correct participation percent and total visitors based on the context provided", function () {
                    var mockParticipationPercentageNode = { textContent: defaultTextContent },
                        mockTotalParticipantNode = { textContent: defaultTextContent };

                    abTestTextHelper.initializeHelper(context);
                    abTestTextHelper.renderVisitorStats(mockParticipationPercentageNode, mockTotalParticipantNode);

                    expect(mockParticipationPercentageNode.textContent).to.equal(50);
                    expect(mockTotalParticipantNode.textContent).to.equal(200);
                });

                it("Renders correct conversion anchor properties based on the context provided",
                    function () {
                        var contentLinkAnchorNode = { href: "badReference", textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderConversion(contentLinkAnchorNode);

                        expect(contentLinkAnchorNode.href).to.equal("testLink");
                        expect(contentLinkAnchorNode.textContent).to.equal("conversion content");
                    });

            });
    });