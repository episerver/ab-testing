define([
    'marketing-testing/TestNotification',
    "epi/i18n!marketing-testing/nls/abtesting"
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
                            state: 0,
                            variants : length = 0
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                {
                        contentData: {
                            contentGuid: "guid",
                            contentLink: "6_168"
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
                            state: 1,
                            variants : length = 0
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid",
                            contentLink: "6_168"
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification.content.innerText).to.have.string(labels.notificationbar.version_in_test);
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
                            state: 2,
                            variants: length = 0
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid",
                            contentLink: "6_168"
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification.content.innerText).to.have.string(labels.notificationbar.completed_test);
            });

            it("don't set a pick winner link when the user does not have access to publish the content under test", function () {
                var aTestNotification = new TestNotification(), aRetNotification;

                aTestNotification._contentActionSupport = {
                    hasAccess: function (accessMask, accessLevel) {
                        return false;
                    },
                    accessLevel: {
                        Publish: "publish"
                    }
                }

                aTestNotification._store = {
                    get: function (contentId) {
                        return this;
                    },
                    then: function (callBack) {
                        var aTest = {
                            id: "a guid",
                            title: "Test Title",
                            state: 2,
                            variants: length = 0
                        }
                        callBack(aTest);
                    }
                };

                aTestNotification._valueSetter(
                    {
                        contentData: {
                            contentGuid: "guid",
                            accessMask: "denied",
                            contentLink: "6_168"
                        }
                    });

                aRetNotification = aTestNotification.get("notification");
                expect(aRetNotification.content.childNodes.length).to.equal(1);
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