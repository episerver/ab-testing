define(["marketing-testing/command/CancelTest"],
    function (CancelTestCommand) {
        describe("CancelTest command", function () {
            it("Is visible when there is a test setup on the current page", function () {
                var me = this,
                    aCommand = new CancelTestCommand(),
                    mockResult = {
                        title: "something clever"
                    },
                    menu = {
                        providers: {
                            length: function () {
                                return 0;
                            },
                        },
                    },
                    store = {
                        get: function (id) {
                            return this;
                        },
                        then: function (successFunc) {
                            successFunc(mockResult);
                            return this;
                        }
                    };
                aCommand.model = {
                    contentData: {
                        contentGuid: "",
                        accessMask: ""
                    }
                }
                aCommand.store = store;
                aCommand.menu = menu;

                aCommand._contentActionSupport = {
                    hasAccess: function (accessMask, targetAccess) {
                        return true;
                    },
                    accessLevel: {
                        Publish: "publish"
                    }
                };
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.true;
                expect(aCommand.canExecute).to.be.true;
            });

            it("Is not visible unless there is a test setup on the current page", function () {
                var me = this,
                    aCommand = new CancelTestCommand(),
                    mockResult = {
                        title: null
                    },
                    menu = {
                        providers: {
                            length: function () {
                                return 0;
                            },
                        },
                    },
                    store = {
                        get: function (id) {
                            return this;
                        },
                        then: function (successFunc) {
                            successFunc(mockResult);
                            return this;
                        }
                    };
                aCommand.model = {
                    contentData: {
                        contentGuid: "",
                        accessMask: ""
                    }
                }
                aCommand.store = store;
                aCommand.menu = menu;
                aCommand._contentActionSupport = {
                    hasAccess: function (accessMask, targetAccess) {
                        return true;
                    },
                    accessLevel: {
                        Publish: "publish"
                    }
                };
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.false;
                expect(aCommand.canExecute).to.be.false;

                mockResult.title = undefined;
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.false;
                expect(aCommand.canExecute).to.be.false;
            });

            it("Is not executable when the user does not have publish permission on the content", function () {
                var me = this,
                    aCommand = new CancelTestCommand(),
                    mockResult = {
                        title: "Good Test",

                    },
                    menu = {
                        providers: {
                            length: function () {
                                return 0;
                            },
                        },
                    },
                    store = {
                        get: function (id) {
                            return this;
                        },
                        then: function (successFunc) {
                            successFunc(mockResult);
                            return this;
                        }
                    };
                aCommand.model = {
                    contentData: {
                        contentGuid: "",
                        accessMask: ""
                    }
                }
                aCommand.store = store;
                aCommand.menu = menu;
                aCommand._contentActionSupport = {
                    hasAccess: function (accessMask, targetAccess) {
                        return false;
                    },
                    accessLevel: {
                        Publish: "publish"
                    }
                };
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.true;
                expect(aCommand.canExecute).to.be.false;
            });

            // Temporary test removal until we can find rogue alert.
            //it("Calls to cancel the current test setup when executed", function () {
            //    var me = this,
            //        aCommand = new CancelTestCommand(),
            //        retPublishView,
            //        mockResult = {
            //            title: "something clever"
            //        },
            //        contentGuid,
            //        store = {
            //            get: function (id) {
            //                return this;
            //            },
            //            then: function (successFunc) {
            //                successFunc();
            //                return this;
            //            },
            //            remove: function (id) {
            //                contentGuid = id;
            //                return this;
            //            }
            //        };

            //    aCommand.store = store;
            //    aCommand.model = {
            //        contentData: {
            //            contentLink: "5_202",
            //            contentGuid: "a test guid",
            //            accessMask: ""
            //        }
            //    };
            //    aCommand._execute();

            //    expect(contentGuid).to.equal(aCommand.model.contentData.contentGuid);
            //});

            it("is visible when the test is done, but a winner has not been picked yet", function () {
                var me = this,
                    aCommand = new CancelTestCommand(),
                    mockResult = {
                        title: "something clever",
                        state: 2
                    },
                    menu = {
                        providers: {
                            length: function () {
                                return 0;
                            },
                        },
                    },
                    store = {
                        get: function (id) {
                            return this;
                        },
                        then: function (successFunc) {
                            successFunc(mockResult);
                            return this;
                        }
                    };

                aCommand.model = {
                    contentData: {
                        contentGuid: "",
                        accessMask: ""
                    }
                }
                aCommand.store = store;
                aCommand.menu = menu;

                aCommand._contentActionSupport = {
                    hasAccess: function (accessMask, targetAccess) {
                        return true;
                    },
                    accessLevel: {
                        Publish: "publish"
                    }
                };
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.true;
                expect(aCommand.canExecute).to.be.true;
            });

        });
    });