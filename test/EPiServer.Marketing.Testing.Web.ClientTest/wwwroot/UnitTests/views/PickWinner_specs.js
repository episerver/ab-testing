define(['marketing-testing/views/PickWinner'],
    function (PickWinner) {
        describe("PickWinner", function () {
            var testData;
            beforeEach(function () {
                testData = {
                    currentContext: {
                        data: {
                            publishedVersionContentLink: "published",
                            draftVersionContentLink: "draft",
                            test: {
                                owner: "a",
                                endDate: "2/12/2022",
                                id: "555555",
                                variants: [{ itemversion: "10", views: 20, conversions: 10 }, { itemversion: "101", views: 50, conversions: 1 }],
                                title: "title",
                                description: "yadda yadda yadda"
                            },
                            publishedVersionName: "pbn",
                            publishedVersionPublishedBy: "me",
                            publishedVersionPublishedDate: "1/1/2008",
                            draftVersionname: "dvn",
                            draftVersionChangedBy: "you",
                            draftVersionChangeDate: "1/5/2015",
                            visitorPercentage: 20,
                            totalParticipantCount: 200,
                            daysElapsed: 5,
                            conversionLink: "5_5",
                            conversionContentName: "blah"
                        }
                    }

                }
            });

            it("_onPublishedVersionClick calls store with published version data", function () {
                var me = this, aPickWinner = new PickWinner(testData), returnedData, retContextCall, retUri;

                aPickWinner.store = {
                    put: function (data) {
                        returnedData = data;
                        return this;
                    },
                    then: function (successfunc) {
                        successfunc();
                        return this;
                    },
                    otherwise: function (otherwiseFunc) {
                    }
                };

                aPickWinner.topic = {
                    publish: function (contextcall, uri) { }
                };

                aPickWinner._onPublishedVersionClick();

                expect(returnedData.publishedContentLink).to.be.equal(testData.currentContext.data.publishedVersionContentLink);
                expect(returnedData.draftContentLink).to.be.equal(testData.currentContext.data.draftVersionContentLink);
                expect(returnedData.winningContentLink).to.be.equal(testData.currentContext.data.publishedVersionContentLink);
                expect(returnedData.testId).to.be.equal(testData.currentContext.data.test.id);
            });

            it("On successful publish of the version redirect to expected destination", function () {
                var me = this, aPickWinner = new PickWinner(testData), returnedData, retContextCall, retUri;

                aPickWinner.store = {
                    put: function (data) {
                        returnedData = data;
                        return this;
                    },
                    then: function (successfunc) {
                        successfunc(5);
                        return this;
                    },
                    otherwise: function (otherwiseFunc) {
                    }
                };

                aPickWinner.topic = {
                    publish: function (contextcall, uri) {
                        retContextCall = contextcall;
                        retUri = uri;
                    }
                };

                aPickWinner._onPublishedVersionClick();
                expect(retContextCall).to.equal("/epi/shell/context/request");
                expect(retUri.uri).to.equal("epi.cms.contentdata:///5");
            });


            it("On successful publish of the variant version redirect to expected destination", function () {
                var me = this, aPickWinner = new PickWinner(testData), returnedData, retContextCall, retUri;

                aPickWinner.store = {
                    put: function (data) {
                        returnedData = data;
                        return this;
                    },
                    then: function (successfunc) {
                        successfunc(5);
                        return this;
                    },
                    otherwise: function (otherwiseFunc) {
                    }
                };

                aPickWinner.topic = {
                    publish: function (contextcall, uri) {
                        retContextCall = contextcall;
                        retUri = uri;
                    }
                };

                aPickWinner._onVariantVersionClick();
                expect(retContextCall).to.equal("/epi/shell/context/request");
                expect(retUri.uri).to.equal("epi.cms.contentdata:///5");
            });

            it("_onVariantVersionClick calls store with published version data", function () {
                var me = this, aPickWinner = new PickWinner(testData), returnedData, retContextCall, retUri;

                aPickWinner.store = {
                    put: function (data) {
                        returnedData = data;
                        return this;
                    },
                    then: function (successfunc) {
                        successfunc();
                        return this;
                    },
                    otherwise: function (otherwiseFunc) {
                    }
                };

                aPickWinner.topic = {
                    publish: function (contextcall, uri) { }
                };

                aPickWinner._onVariantVersionClick();

                expect(returnedData.publishedContentLink).to.be.equal(testData.currentContext.data.publishedVersionContentLink);
                expect(returnedData.draftContentLink).to.be.equal(testData.currentContext.data.draftVersionContentLink);
                expect(returnedData.winningContentLink).to.be.equal(testData.currentContext.data.draftVersionContentLink);
                expect(returnedData.testId).to.be.equal(testData.currentContext.data.test.id);
            });
        });
    });
    