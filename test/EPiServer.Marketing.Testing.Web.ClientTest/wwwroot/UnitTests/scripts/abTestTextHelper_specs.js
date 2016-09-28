define([
        'marketing-testing/scripts/abTestTextHelper',
        'epi/_Module'
],
    function (abTestTextHelper, Module) {
        describe("abTestTextHelper - Details View",
            function () {
                var mockStringResources = {
                    test_status_running: "Test is running, ",
                    days_remaining: "day(s) remaining.",
                    started: "started ",
                    test_status_completed: "Test completed, now go on and pick a winner...",
                    test_status_not_started: "Test has not yet started, ",
                    test_scheduled: "It is scheduled to begin ",
                    by: "by",
                    result_is_not_significant: "The results of this test are NOT significant.",
                    result_is_significant: "The results of this test are significant.",
                    early_pick_winner_message:
                        "This test has not been completed, but you may pick a winner. Picking a winner now will end the test and publish the content chosen."
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
                context.data.test.variants.push({ itemVersion: "3", isPublished: true,conversions: 10, views: 100 });
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

                        expect(mockStatusElement.textContent).to.equal("Test has not yet started, ");
                        expect(mockStartedElement.textContent).to.equal("It is scheduled to begin Aug 3, 3:27 PM")

                    });

                it("Renders correct status and messaging for a test which is active (state = 1)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };
                        context.data.test.state = 1;

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test is running, 26 day(s) remaining.");
                        expect(mockStartedElement.textContent).to.equal("started Aug 3, 3:27 PM by MockTestOwner");

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

                it("Renders correct timeRemaining based on the context provided",
                    function () {
                        var mockDaysRemainingElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderTestRemaining(mockDaysRemainingElement);

                        expect(mockDaysRemainingElement.textContent).to.equal(26);
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
                        expect(mockDatePublishedElement.textContent).to.equal("May 1, 8:00 PM");
                    });

                it("Renders correct draft(challenger) content information based on the context provided",
                    function () {
                        var mockChangedByElement = { textContent: defaultTextContent },
                            mockdateChangedElement = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context, mockStringResources, mockDependencies);
                        abTestTextHelper.renderDraftInfo(mockChangedByElement, mockdateChangedElement);

                        expect(mockChangedByElement.textContent).to.equal("MockChangedByUser");
                        expect(mockdateChangedElement.textContent).to.equal("May 2, 9:00 PM");
                    });

                it("Renders correct published(control) variant views, conversions and conversion percent based on the context provided",
                    function () {
                        var mockPublishedConversionsNode = { textContent: defaultTextContent },
                            mockPublishedViewsNode = { textContent: defaultTextContent },
                            mockPublishedConversionPercentNode = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderPublishedViewsAndConversions(mockPublishedConversionsNode,
                            mockPublishedViewsNode,
                            mockPublishedConversionPercentNode);

                        expect(mockPublishedConversionsNode.textContent).to.equal(10);
                        expect(mockPublishedViewsNode.textContent).to.equal(100);
                        expect(mockPublishedConversionPercentNode.textContent).to.equal("10%");

                    });

                it("Renders correct draft(challenger) variant views, conversions and conversion percent based on the context provided",
                    function () {
                        var mockchallengerConversionsNode = { textContent: defaultTextContent },
                            mockchallengerViewsNode = { textContent: defaultTextContent },
                            mockchallengerConversionPercentNode = { textContent: defaultTextContent };

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderDraftViewsAndConversions(mockchallengerConversionsNode,
                            mockchallengerViewsNode,
                            mockchallengerConversionPercentNode);

                        expect(mockchallengerConversionsNode.textContent).to.equal(150);
                        expect(mockchallengerViewsNode.textContent).to.equal(200);
                        expect(mockchallengerConversionPercentNode.textContent).to.equal("75%");
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

                        expect(mockDescriptionNode.textContent).to.equal("");

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

                it("Informs user test is not significant when test is done and significance is false",
                    function () {
                        var pickAWinnerMessageNode = { innerHTML: defaultTextContent };
                        context.data.test.state = 2;
                        context.data.test.isSignificant = false;

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderSignificance(pickAWinnerMessageNode);

                        expect(pickAWinnerMessageNode.innerHTML)
                            .to.equal("The results of this test are NOT significant.");
                    });

                it("Informs user test is significant when test is done and significance is true",
                    function () {
                        var pickAWinnerMessageNode = { innerHTML: defaultTextContent };
                        context.data.test.state = 2;
                        context.data.test.isSignificant = true;

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderSignificance(pickAWinnerMessageNode);

                        expect(pickAWinnerMessageNode.innerHTML).to.equal("The results of this test are significant.");
                    });

                it("Informs user a winner may be chosen even though the test is not complete if on the pick a winner page",
                    function () {
                        var mockPickAWinnerMessageNode = { innerHTML: defaultTextContent };

                        context.data.test.state = 1;

                        abTestTextHelper.initializeHelper(context);
                        abTestTextHelper.renderSignificance(mockPickAWinnerMessageNode);

                        expect(mockPickAWinnerMessageNode.innerHTML)
                            .to
                            .equal("This test has not been completed, but you may pick a winner. Picking a winner now will end the test and publish the content chosen.");
                    });
            });
    });