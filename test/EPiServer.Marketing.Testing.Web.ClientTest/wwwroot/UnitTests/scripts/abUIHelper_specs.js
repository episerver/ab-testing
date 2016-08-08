define(['marketing-testing/scripts/abUIHelper',
        'epi/_Module'],
    function (uiHelper, Module) {
        describe("abUIHelper",
            function () {
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
                context.data.test.variants.push({ itemVersion: "3", conversions: 10, views: 100 });
                context.data.test.variants.push({ itemVersion: "150", conversions: 150, views: 200 });

                it("Correctly initializes properties based on given context",
                    function () {
                        uiHelper.initializeHelper(context);
                        expect(uiHelper.publishedVariant.itemVersion).to.equal("3");
                        expect(uiHelper.publishedVariant.conversions).to.equal(10);
                        expect(uiHelper.publishedVariant.views).to.equal(100);
                        expect(uiHelper.draftVariant.itemVersion).to.equal("150");
                        expect(uiHelper.draftVariant.conversions).to.equal(150);
                        expect(uiHelper.draftVariant.views).to.equal(200);
                        expect(uiHelper.publishedPercent).to.equal(10);
                        expect(uiHelper.draftPercent).to.equal(75)
                    });

                it("Correctly sets the textContent of the provided title element",
                    function () {
                        var mockTitleElement = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context);
                        uiHelper.renderTitle(mockTitleElement);

                        expect(mockTitleElement.textContent).to.equal("Unit Test Title");
                    });

                it("Renders correct status and messaging for a test which is not started (state = 0)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context);
                        uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test has not yet started, ");
                        expect(mockStartedElement.textContent).to.equal("It is scheduled to begin Aug 3, 3:27 PM")

                    });

                it("Renders correct status and messaging for a test which is active (state = 1)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };
                        context.data.test.state = 1;

                        uiHelper.initializeHelper(context, mockDependencies);
                        uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test is running, 26 day(s) remaining.");
                        expect(mockStartedElement.textContent).to.equal("started Aug 3, 3:27 PM by MockTestOwner");

                    });

                it("Renders correct status and messaging for a test which is done (state = 2)",
                    function () {
                        var mockStatusElement = { textContent: defaultTextContent };
                        var mockStartedElement = { textContent: defaultTextContent };
                        context.data.test.state = 2;

                        uiHelper.initializeHelper(context, mockDependencies);
                        uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                        expect(mockStatusElement.textContent).to.equal("Test completed, no go on and pick a winner...");
                        expect(mockStartedElement.textContent).to.equal("");

                    });

                it("Renders correct daysElapsed based on the context provided",
                    function () {
                        var mockDaysElapsedElement = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context);
                        uiHelper.renderTestDuration(mockDaysElapsedElement);

                        expect(mockDaysElapsedElement.textContent).to.equal(4);
                    });

                it("Renders correct timeRemaining based on the context provided",
                    function () {
                        var mockDaysRemainingElement = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context);
                        uiHelper.renderTestRemaining(mockDaysRemainingElement);

                        expect(mockDaysRemainingElement.textContent).to.equal(26);
                    });

                it("Renders correct confidence based on the context provided",
                    function () {
                        var mockConfidenceLevelElement = { defaultTextContent };

                        uiHelper.initializeHelper(context);
                        uiHelper.renderConfidence(mockConfidenceLevelElement);

                        expect(mockConfidenceLevelElement.textContent).to.equal("98%");
                    });

                it("Renders correct published(control) content information based on the context provided",
                    function () {
                        var mockPublishedElement = { textContent: defaultTextContent },
                            mockDatePublishedElement = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context, mockDependencies);
                        uiHelper.renderPublishedInfo(mockPublishedElement, mockDatePublishedElement);

                        expect(mockPublishedElement.textContent).to.equal("MockPublishedUser");
                        expect(mockDatePublishedElement.textContent).to.equal("May 1, 8:00 PM");
                    });

                it("Renders correct draft(challenger) content information based on the context provided",
                    function () {
                        var mockChangedByElement = { textContent: defaultTextContent },
                            mockdateChangedElement = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context, mockDependencies);
                        uiHelper.renderDraftInfo(mockChangedByElement, mockdateChangedElement);

                        expect(mockChangedByElement.textContent).to.equal("MockChangedByUser");
                        expect(mockdateChangedElement.textContent).to.equal("May 2, 9:00 PM");
                    });

                it("Renders correct published(control) variant views, conversions and conversion percent based on the context provided",
                    function () {
                        var mockPublishedConversionsNode = { textContent: defaultTextContent },
                            mockPublishedViewsNode = { textContent: defaultTextContent },
                            mockPublishedConversionPercentNode = { textContent: defaultTextContent };

                        uiHelper.initializeHelper(context);
                        uiHelper.renderPublishedViewsAndConversions(mockPublishedConversionsNode,
                                mockPublishedViewsNode,
                                mockPublishedConversionPercentNode);

                        expect(mockPublishedConversionsNode.textContent).to.equal(10);
                        expect(mockPublishedViewsNode.textContent).to.equal(100);
                        expect(mockPublishedConversionPercentNode.textContent).to.equal("10%");

                    });

                it("Renders correct draft(challenger) variant views, conversions and conversion percent based on the context provided", function () {
                    var mockchallengerConversionsNode = { textContent: defaultTextContent },
                        mockchallengerViewsNode = { textContent: defaultTextContent },
                        mockchallengerConversionPercentNode = { textContent: defaultTextContent };

                    uiHelper.initializeHelper(context);
                    uiHelper.renderDraftViewsAndConversions(mockchallengerConversionsNode,
                            mockchallengerViewsNode,
                            mockchallengerConversionPercentNode);

                    expect(mockchallengerConversionsNode.textContent).to.equal(150);
                    expect(mockchallengerViewsNode.textContent).to.equal(200);
                    expect(mockchallengerConversionPercentNode.textContent).to.equal("75%");
                });

                it("Renders a properly formatted description when description is not null", function () {
                    var mockDescriptionNode = { textContent: defaultTextContent };

                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderDescription(mockDescriptionNode);

                    expect(mockDescriptionNode.textContent).to.equal("\"Mock Test Description\" - MockTestOwner");
                });

                it("Renders description as an empty string when description is null", function () {
                    var mockDescriptionNode = { textContent: defaultTextContent };
                    context.data.test.description = null;

                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderDescription(mockDescriptionNode);

                    expect(mockDescriptionNode.textContent).to.equal("");

                });

                it("Renders proper control and challenger styles when conversions percents are equal", function () {
                    var mockControlStatusIconNode = { mockClass: defaultTextContent },
                        mockChallengerStatusIconNode = { mockClass: defaultTextContent },
                        mockControlWrapperNode = { mockClass: defaultTextContent },
                        mockCallengerWrapperNode = { mockClass: defaultTextContent };
                    context.data.test.variants[0].views = 2;
                    context.data.test.variants[0].conversions = 2;
                    context.data.test.variants[1].views = 2;
                    context.data.test.variants[1].conversions = 2;
                    context.data.test.state = 1;

                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderStatusIndicatorStyles(mockControlStatusIconNode,
                        mockChallengerStatusIconNode,
                        mockControlWrapperNode,
                        mockCallengerWrapperNode);

                    expect(mockControlStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockChallengerStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockControlWrapperNode.mockClass).to.equal("cardWrapper 2column controlDefaultBody");
                    expect(mockCallengerWrapperNode.mockClass).to.equal("cardWrapper 2column challengerDefaultBody");
                });

                it("Renders proper control and challenger styles when control conversion percent is higher and test is not done ", function () {
                    var mockControlStatusIconNode = { mockClass: defaultTextContent },
                        mockChallengerStatusIconNode = { mockClass: defaultTextContent },
                        mockControlWrapperNode = { mockClass: defaultTextContent },
                        mockCallengerWrapperNode = { mockClass: defaultTextContent };
                    context.data.test.variants[0].views = 100;
                    context.data.test.variants[0].conversions = 75;
                    context.data.test.variants[1].views = 100;
                    context.data.test.variants[1].conversions = 25;
                    context.data.test.state = 1;
                    
                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderStatusIndicatorStyles(mockControlStatusIconNode,
                        mockChallengerStatusIconNode,
                        mockControlWrapperNode,
                        mockCallengerWrapperNode);

                    expect(mockControlStatusIconNode.mockClass).to.equal("leadingContent");
                    expect(mockChallengerStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockControlWrapperNode.mockClass).to.equal("cardWrapper 2column controlLeaderBody");
                    expect(mockCallengerWrapperNode.mockClass).to.equal("cardWrapper 2column challengerDefaultBody");
                });

                it("Renders proper control and challenger styles when challenger conversion percent is higher and test is not done", function () {
                    var mockControlStatusIconNode = { mockClass: defaultTextContent },
                        mockChallengerStatusIconNode = { mockClass: defaultTextContent },
                        mockControlWrapperNode = { mockClass: defaultTextContent },
                        mockCallengerWrapperNode = { mockClass: defaultTextContent };

                    context.data.test.variants[0].views = 100;
                    context.data.test.variants[0].conversions = 25;
                    context.data.test.variants[1].views = 100;
                    context.data.test.variants[1].conversions = 50;
                    context.data.test.state = 1;
                    
                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderStatusIndicatorStyles(mockControlStatusIconNode,
                        mockChallengerStatusIconNode,
                        mockControlWrapperNode,
                        mockCallengerWrapperNode);

                    expect(mockControlStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockChallengerStatusIconNode.mockClass).to.equal("leadingContent");
                    expect(mockControlWrapperNode.mockClass).to.equal("cardWrapper 2column controlTrailingBody");
                    expect(mockCallengerWrapperNode.mockClass).to.equal("cardWrapper 2column challengerLeaderBody");
                });

                it("Renders proper control and challenger styles when control conversion percent is higher and test is finished ", function () {
                    var mockControlStatusIconNode = { mockClass: defaultTextContent },
                        mockChallengerStatusIconNode = { mockClass: defaultTextContent },
                        mockControlWrapperNode = { mockClass: defaultTextContent },
                        mockCallengerWrapperNode = { mockClass: defaultTextContent };

                    context.data.test.variants[0].views = 100;
                    context.data.test.variants[0].conversions = 50;
                    context.data.test.variants[1].views = 100;
                    context.data.test.variants[1].conversions = 10;
                    context.data.test.state = 2;
                    
                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderStatusIndicatorStyles(mockControlStatusIconNode,
                        mockChallengerStatusIconNode,
                        mockControlWrapperNode,
                        mockCallengerWrapperNode);

                    expect(mockControlStatusIconNode.mockClass).to.equal("winningContent");
                    expect(mockChallengerStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockControlWrapperNode.mockClass).to.equal("cardWrapper 2column controlLeaderBody");
                    expect(mockCallengerWrapperNode.mockClass).to.equal("cardWrapper 2column challengerDefaultBody");
                });

                it("Renders proper control and challenger styles when challenger conversion percent is higher and test finished", function () {
                    var mockControlStatusIconNode = { mockClass: defaultTextContent },
                        mockChallengerStatusIconNode = { mockClass: defaultTextContent },
                        mockControlWrapperNode = { mockClass: defaultTextContent },
                        mockCallengerWrapperNode = { mockClass: defaultTextContent };
                    context.data.test.variants[0].views = 100;
                    context.data.test.variants[0].conversions = 25;
                    context.data.test.variants[1].views = 100;
                    context.data.test.variants[1].conversions = 75;
                    context.data.test.state = 2;

                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderStatusIndicatorStyles(mockControlStatusIconNode,
                        mockChallengerStatusIconNode,
                        mockControlWrapperNode,
                        mockCallengerWrapperNode);

                    expect(mockControlStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockChallengerStatusIconNode.mockClass).to.equal("winningContent");
                    expect(mockControlWrapperNode.mockClass).to.equal("cardWrapper 2column controlTrailingBody");
                    expect(mockCallengerWrapperNode.mockClass).to.equal("cardWrapper 2column challengerLeaderBody");
                });

                it("Renders shadow styles when pickawinner is set", function () {
                    var mockControlStatusIconNode = { mockClass: defaultTextContent },
                        mockChallengerStatusIconNode = { mockClass: defaultTextContent },
                        mockControlWrapperNode = { mockClass: defaultTextContent },
                        mockCallengerWrapperNode = { mockClass: defaultTextContent };
                    context.data.test.variants[0].views = 100;
                    context.data.test.variants[0].conversions = 25;
                    context.data.test.variants[1].views = 100;
                    context.data.test.variants[1].conversions = 75;
                    context.data.test.state = 2;

                    uiHelper.initializeHelper(context, mockDependencies);
                    uiHelper.renderStatusIndicatorStyles(mockControlStatusIconNode,
                        mockChallengerStatusIconNode,
                        mockControlWrapperNode,
                        mockCallengerWrapperNode, "true");

                    expect(mockControlStatusIconNode.mockClass).to.equal("noIndicator");
                    expect(mockChallengerStatusIconNode.mockClass).to.equal("winningContent");
                    expect(mockControlWrapperNode.mockClass).to.equal("cardWrapperShadowed 2column controlTrailingBody");
                    expect(mockCallengerWrapperNode.mockClass).to.equal("cardWrapperShadowed 2column challengerLeaderBody");
                });

                it("Renders correct participation percent and total visitors based on the context provided", function () {
                    var mockParticipationPercentageNode = { textContent: defaultTextContent },
                        mockTotalParticipantNode = { textContent: defaultTextContent };

                    uiHelper.initializeHelper(context);
                    uiHelper.renderVisitorStats(mockParticipationPercentageNode, mockTotalParticipantNode);

                    expect(mockParticipationPercentageNode.textContent).to.equal(50);
                    expect(mockTotalParticipantNode.textContent).to.equal(200);
                });

                it("Renders correct conversion anchor properties based on the context provided", function () {
                    var contentLinkAnchorNode = { href: "badReference", textContent: defaultTextContent };

                    uiHelper.initializeHelper(context);
                    uiHelper.renderConversion(contentLinkAnchorNode);

                    expect(contentLinkAnchorNode.href).to.equal("testLink");
                    expect(contentLinkAnchorNode.textContent).to.equal("conversion content");
                });

                it("Informs user test is not significant when test is done and significance is false", function () {
                    var pickAWinnerMessageNode = { innerHTML: defaultTextContent };
                    context.data.test.state = 2;
                    context.data.test.isSignificant = false;

                    uiHelper.initializeHelper(context);
                    uiHelper.renderSignificance(pickAWinnerMessageNode);

                    expect(pickAWinnerMessageNode.innerHTML).to.equal("The results of this test are NOT significant.");
                });

                it("Informs user test is significant when test is done and significance is true", function () {
                    var pickAWinnerMessageNode = { innerHTML: defaultTextContent };
                    context.data.test.state = 2;
                    context.data.test.isSignificant = true;
                    
                    uiHelper.initializeHelper(context);
                    uiHelper.renderSignificance(pickAWinnerMessageNode);

                    expect(pickAWinnerMessageNode.innerHTML).to.equal("The results of this test are significant.");
                });

                it("Informs user a winner may be chosen even though the test is not complete if on the pick a winner page", function () {
                    var mockPickAWinnerMessageNode = { innerHTML: defaultTextContent };

                    context.data.test.state = 1;
                    
                    uiHelper.initializeHelper(context);
                    uiHelper.renderSignificance(mockPickAWinnerMessageNode);

                    expect(mockPickAWinnerMessageNode.innerHTML).to.equal("This test has not been completed, but you may pick a winner. Picking a winner now will end the test and publish the content chosen.");
                });
            });
    });