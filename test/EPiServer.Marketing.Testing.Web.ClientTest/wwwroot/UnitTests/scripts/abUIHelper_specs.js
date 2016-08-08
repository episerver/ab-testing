define(['marketing-testing/scripts/abUIHelper',
        'epi/_Module'],
    function (uiHelper, Module) {
        describe("abUIHelper", function () {
            var context = {
                data: {
                    publishedVersionContentLink: "3",
                    test: {
                        title: "Unit Test Title",
                        startDate: "2016-08-03 15:27:50.000",
                        endDate: "2016-09-02 15:27:50.000",
                        state: 0,
                        variants: []
                    }
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

            it("Renders correct status and messaging for a test which is active (state = 1)", function(){
                var testContext = context;
                var mUserModule = {
                    toUserFriendlyString: function(){ return "MockUser"}
                };
                
                var mockStatusElement = { textContent: "MOCK STATUS" };
                var mockStartedElement = { textContent: "MOCK STARTED" };
                testContext.data.test.state = 1;

                var now = new Date();
                var end = new Date(testContext.data.test.endDate);
                var daysRemaining = Math.ceil(((end - now) / 1000)/(24*60*60));

                uiHelper.initializeHelper(testContext,mUserModule);
                uiHelper.renderTestStatus(mockStatusElement, mockStartedElement);

                expect(mockStatusElement.textContent).to.equal("Test is running, "+daysRemaining+" day(s) remaining ");
                expect(mockStartedElement.textContent).to.equal("It is scheduled to begin Aug 3, 3:27 PM")

            });

            it("Renders correct status and messaging for a test which is done (state = 2)");

            it("Renders correct daysElapsed based on the context provided");

            it("Renders correct timeRemaining based on the context provided");

            it("Renders correct confidence based on the context provided");

            it("Renders correct published(control) content information based on the context provided");

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