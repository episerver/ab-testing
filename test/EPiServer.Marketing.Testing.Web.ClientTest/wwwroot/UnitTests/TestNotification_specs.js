define([
    'marketing-testing/TestNotification',
    "dojo/i18n!marketing-testing/nls/MarketingTestingLabels"
],
    function (TestNotification, labels) {
        describe("TestNotification", function () {
            it("sets a notification when there is a pending test on the content", function () {
                var aTestNotification = new TestNotification(), aRetNotification;

                aTestNotification._store = {
                    get: function (contentId) {
                        return this;
                    },
                    then: function (callBack) {
                        var aTest = {
                            id: "a guid",
                            title: "Test Title",
                            state: 0
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid"                             
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification.content.innerText).to.have.string(labels.notificationbar.scheduled_test);
            });

            it("sets a notification when there is an active test on the content", function () {
                var aTestNotification = new TestNotification(), aRetNotification;

                aTestNotification._store = {
                    get: function (contentId) {
                        return this;
                    },
                    then: function (callBack) {
                        var aTest = {
                            id: "a guid",
                            title: "Test Title",
                            state: 1
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid"
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification.content.innerText).to.have.string(labels.notificationbar.ongoing_test);
            });

            it("sets a notification when a test is done, but a winner is not picked on the content", function () {
                var aTestNotification = new TestNotification(), aRetNotification;

                aTestNotification._store = {
                    get: function (contentId) {
                        return this;
                    },
                    then: function (callBack) {
                        var aTest = {
                            id: "a guid",
                            title: "Test Title",
                            state: 2
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid"
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification.content.innerText).to.have.string(labels.notificationbar.completed_test);
            });

            it("does not set a notification when there is no test on the content", function () {
                var aTestNotification = new TestNotification(), aRetNotification;

                aTestNotification._store = {
                    get: function (contentId) {
                        return this;
                    },
                    then: function (callBack) {
                        var aTest = {
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid"
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification).to.be.null;
            });
        });
    });