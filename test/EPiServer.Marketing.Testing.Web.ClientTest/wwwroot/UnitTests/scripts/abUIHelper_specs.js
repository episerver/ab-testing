define(['marketing-testing/scripts/abUIHelper',
        'epi/_Module'],

    function (uiHelper, Module) {
        describe("abUIHelper", function () {
            var context = {
                data: {
                    daysElapsed: 4,
                    daysRemaining: 26,
                    publishedVersionPublishedBy: "MockPublishedUser",
                    publishedVersionPublishedDate: "2016-05-01 20:00:00.000",
                    publishedVersionContentLink: "3",
                    test: {
                        title: "Unit Test Title",
                        startDate: "2016-08-03 15:27:50.000",
                        endDate: "2016-09-02 15:27:50.000",
                        state: 0,
                        variants: [],
                        confidenceLevel: 98,
                        owner: "MockTestOwner"
                    }
                }
            };

            var mUserModule = {
                toUserFriendlyString: function(name) {
                    return name;
                }
            };

            context.data.test.variants.push({ itemVersion: "3", conversions: 10, views: 100 });
            context.data.test.variants.push({ itemVersion: "150", conversions: 150, views: 200 });

            it("Correctly initializes properties based on given context", function () {
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

            it("Correctly sets the textContent of the provided title element", function () {

                uiHelper.initializeHelper(context);
                var mockTitleElement = { textContent: "mockTitle" };
                uiHelper.renderTitle(mockTitleElement);
                expect(mockTitleElement.textContent).to.equal("Unit Test Title");
            });

            it("Renders correct status and messaging for a test which is not started (state = 0)", function () {
                var mockStatusElement = { textContent: "MOCK STATUS" };
                var mockStartedElement = { textContent: "MOCK STARTED" };
                uiHelper.initializeHelper(context);
                uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                expect(mockStatusElement.textContent).to.equal("Test has not yet started, ");
                expect(mockStartedElement.textContent).to.equal("It is scheduled to begin Aug 3, 3:27 PM")

            });

            it("Renders correct status and messaging for a test which is active (state = 1)", function () {
                var testContext = context;

                var mockStatusElement = { textContent: "MOCK STATUS" };
                var mockStartedElement = { textContent: "MOCK STARTED" };
                testContext.data.test.state = 1;

                uiHelper.initializeHelper(testContext, mUserModule);
                uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                expect(mockStatusElement.textContent).to.equal("Test is running, 26 day(s) remaining.");
                expect(mockStartedElement.textContent).to.equal("started Aug 3, 3:27 PM by MockTestOwner");

            });

            it("Renders correct status and messaging for a test which is done (state = 2)", function () {
                var testContext = context;

                var mockStatusElement = { textContent: "MOCK STATUS" };
                var mockStartedElement = { textContent: "MOCK STARTED" };
                testContext.data.test.state = 2;

                uiHelper.initializeHelper(testContext, mUserModule);
                uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                expect(mockStatusElement.textContent).to.equal("Test completed, no go on and pick a winner...");
                expect(mockStartedElement.textContent).to.equal("");

            });

            it("Renders correct daysElapsed based on the context provided", function () {
                var mockDaysElapsedElement = { textContent: "100" };

                uiHelper.initializeHelper(context);
                uiHelper.renderTestDuration(mockDaysElapsedElement);

                expect(mockDaysElapsedElement.textContent).to.equal(4);
            });

            it("Renders correct timeRemaining based on the context provided", function () {
                var mockDaysRemainingElement = { textContent: "100" };

                uiHelper.initializeHelper(context);
                uiHelper.renderTestRemaining(mockDaysRemainingElement);

                expect(mockDaysRemainingElement.textContent).to.equal(26);
            });

            it("Renders correct confidence based on the context provided", function () {
                var mockConfidenceLevelElement = { textContent: "0" };

                uiHelper.initializeHelper(context);
                uiHelper.renderConfidence(mockConfidenceLevelElement);

                expect(mockConfidenceLevelElement.textContent).to.equal("98%");
            });

            it("Renders correct published(control) content information based on the context provided", function () {
                var mockPublishedElement = { textContent: "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                    mockDatePublishedElement = { textContent: "ABCDEFGHIJKLMNOPQRSTUVWXYZ" };

                uiHelper.initializeHelper(context, mUserModule);
                uiHelper.renderPublishedInfo(mockPublishedElement, mockDatePublishedElement);
            });

            it("Renders correct draft(challenger) content information based on the context provided");

            it("Renders correct published(control) variant views, conversions and conversion percent based on the context provided");

            it("Renders correct draft(challenger) variant views, conversions and conversion percent based on the context provided");

            it("Renders a properly formatted description when description is not null");

            it("Renders description as an empty string when description is null");

            it("Renders proper control and challenger styles when conversions percents are equal");

            it("Renders proper control and challenger styles when control conversions percents are higher and test is not done ");

            it("Renders proper control and challenger styles when challenger conversoins are higher and test is not done");

            it("Renders proper control and challenger styles when control conversions percents are higher and test is finished ");

            it("Renders proper control and challenger styles when challenger conversoins are higher and test finished");

            it("Renders correct participation percent and total visitors based on the context provided");

            it("Renders correct conversion anchor properties based on the context provided");

            it("Informs user test is not significant when test is done and significance is false");

            it("Informs user test is significant when test is done and significance is true");

            it("Informs user a winner may be chosen even though the test is not complete if on the pick a winner page");

            it("Generates a pie chart if a piechart node is available");

            it("Does not generate a pie chart if picechart node is unavailable");
        });
    });