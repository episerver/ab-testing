define(['marketing-testing/viewmodels/AddTestViewModel',
        'epi/_Module'],
    function (AddTestViewModel, Module) {
        describe("AddTestViewModel", function () {
            it("sets up the object dependencies on postscript", function () {
                var me = this,
                aGuidString = "5F0B06A7-12C0-405F-8F55-C60D7187AB34",
                aPrefixString = "6",
                aVersionString = "168",
                aDraftVersionString = "169",

                aContentResult = {
                    contentLink: aPrefixString + "_" + aVersionString,
                    contentGuid: aGuidString
                },
                aContentData = {
                    contentLink: aPrefixString + "_" + aDraftVersionString,
                    contentGuid: aGuidString
                },
                aContentVersionStore = {
                    query: function (queryObject) {
                        me.retQueryObj = queryObject;
                        return this;
                    },
                    then: function (successFunction) {
                        successFunction(aContentResult);
                        return this;
                    },
                    otherwise: function (otherwiseFunction) {
                        return this;
                    }
                },
                aLanguageContext = {
                    language: "mycontentlanguage"
                },

                aViewModel = AddTestViewModel({
                    store: {},
                    _contentVersionStore: aContentVersionStore,
                    contentData: aContentData,
                    languageContext: aLanguageContext
                });

                expect(aViewModel.publishedVersion).to.equal(aContentResult);
                expect(me.retQueryObj.contentLink).to.equal(aContentData.contentLink);
                expect(aViewModel.currentVersion).to.equal(aContentData);
            });

            it("creates a test with the data stored in the view model", function () {
                var me = this,
                aGuidString = "5F0B06A7-12C0-405F-8F55-C60D7187AB34",
                aPrefixString = "6",
                aVersionString = "168",
                aDraftVersionString = "169",

                aContentResult = {
                    contentLink: aPrefixString + "_" + aVersionString,
                    contentGuid: aGuidString
                },
                aContentData = {
                    contentLink: aPrefixString + "_" + aDraftVersionString,
                    contentGuid: aGuidString
                },

                aContentVersionStore = {
                    query: function (queryObject) {
                        me.retQueryObj = queryObject;
                        return this;
                    },
                    then: function (successFunction) {
                        successFunction(aContentResult);
                        return this;
                    },
                    otherwise: function (otherwiseFunction) {
                        return this;
                    }
                },
                aLanguageContext = {
                    language: "mycontentlanguage"
                },

                aViewModel = AddTestViewModel({
                    store: {
                        put: function (data) {
                            me.retTest = data;
                            return this;
                        },
                        then: function(successFunction) {
                            return this;
                        },
                        otherwise: function(otherwiseFunction) {
                        }
                    },

                    _contentVersionStore: aContentVersionStore,
                    contentData: aContentData,
                    languageContext: aLanguageContext
                });

                aViewModel.currentVersion = aContentData;
                aViewModel.publishedVersion = aContentResult;
                aViewModel.testTitle = "title";
                aViewModel.testDescription = "desc";
                aViewModel.conversionPage = "kpiPage";
                aViewModel.participationPercent = 55;
                aViewModel.testDuration = 7;
                aViewModel.startDate = "today";

                aViewModel.createTest();

                expect(me.retTest.testDescription).to.equal(aViewModel.testDescription);
                expect(me.retTest.testContentId).to.equal(aContentData.contentGuid);
                expect(me.retTest.publishedVersion).to.equal(aVersionString);
                expect(me.retTest.variantVersion).to.equal(aDraftVersionString);
                expect(me.retTest.testDuration).to.equal(aViewModel.testDuration);
                expect(me.retTest.participationPercent).to.equal(aViewModel.participationPercent);
                expect(me.retTest.conversionPage).to.equal(aViewModel.conversionPage);
                expect(me.retTest.testTitle).to.equal(aViewModel.testTitle);
                expect(me.retTest.startDate).to.equal(aViewModel.startDate);
            });

            it("goes back to the previous page on a successful test creation", function () {
                var me = this, retCall, retParams,

                aGuidString = "5F0B06A7-12C0-405F-8F55-C60D7187AB34",
                aPrefixString = "6",
                aVersionString = "168",
                aDraftVersionString = "169",

                aContentResult = {
                    contentLink: aPrefixString + "_" + aVersionString,
                    contentGuid: aGuidString,
                    contentLink: aPrefixString
                },
                aContentData = {
                    contentLink: aPrefixString + "_" + aDraftVersionString,
                    contentGuid: aGuidString,
                    contentLink: aPrefixString
                },

                aContentVersionStore = {
                    query: function (queryObject) {
                        return this;
                    },
                    then: function (successFunction) {
                        return this;
                    },
                    otherwise: function (otherwiseFunction) {
                        return this;
                    }
                },

                aLanguageContext = {
                    language: "mycontentlanguage"
                };
                var aViewModel = AddTestViewModel({
                    store: {
                        put: function (data) {
                            return this;
                        },
                        then: function (successFunction) {
                            successFunction();
                            return this;
                        },
                        otherwise: function (otherwiseFunction) {
                        }
                    },
                    topic: {
                        publish: function (action, params) {
                            me.retCall = action;
                            me.retParams = params;
                            return this;
                        }
                    },

                    _contentVersionStore: aContentVersionStore,
                    contentData: aContentData,
                    languageContext: aLanguageContext
                });

                aViewModel.currentVersion = aContentData;
                aViewModel.publishedVersion = aContentResult;
                aViewModel.createTest();
                expect(me.retCall).to.equal("/epi/shell/context/request");
                expect(me.retParams.uri).to.equal("epi.cms.contentdata:///" + aPrefixString);
            });
        });
    });