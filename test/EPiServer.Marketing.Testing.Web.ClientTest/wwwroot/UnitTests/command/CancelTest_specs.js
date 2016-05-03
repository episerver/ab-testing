define(["marketing-testing/command/CancelTest"],
    function (CancelTestCommand) {
        describe("CancelTest command", function () {
            it("Is visible when there is a test setup on the current page", function () {
                var me = this,
                    aCommand = new CancelTestCommand(),
                    mockResult = {
                        title: "something clever"
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
                        contentGuid: ""
                    }
                }
                aCommand.store = store;
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
                        contentGuid: ""
                    }
                }
                aCommand.store = store;
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.false;
                expect(aCommand.canExecute).to.be.false;

                mockResult.title = undefined;
                aCommand._onModelChange();

                expect(aCommand.isAvailable).to.be.false;
                expect(aCommand.canExecute).to.be.false;
            });

            it("Calls to cancel the current test setup when executed", function () {
                var me = this,
                    aCommand = new CancelTestCommand(),
                    retPublishView,
                    mockResult = {
                        title: "something clever"
                    },
                    contentGuid,
                    store = {
                        get: function (id) {
                            return this;
                        },
                        then: function (successFunc) {
                            successFunc();
                            return this;
                        },
                        remove: function (id) {
                            contentGuid = id;
                            return this;
                        }
                    };

                aCommand.store = store;
                aCommand.model = {
                    contentData: {
                        contentGuid: "a test guid"
                    }
                };
                aCommand._execute();

                expect(contentGuid).to.equal(aCommand.model.contentData.contentGuid);
            });
        });
    });