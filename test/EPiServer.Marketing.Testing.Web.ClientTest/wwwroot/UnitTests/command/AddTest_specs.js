define(["marketing-testing/command/AddTest"],
    function (AddTestCommand) {
        describe("AddTest command", function () {
        var contentActionSupport = {
            versionStatus: {
                Published: "Published",
                Expired: "Expired",
                CheckedOut: "CheckedOut",
                Rejected: "Rejected"
            },
            accessLevel: {
                Publish: ""
            },
            hasAccess: function (a, b) { return true }
        };

        it("is viewable when the content is in a draft state", function () {
            var aCommand = new AddTestCommand()
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "CheckedOut",
                    isCommonDraft: true,
                    publishedBy: "Timmy",
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand._onModelChange()
            expect(aCommand.isAvailable).to.be.true;

            aCommand.isAvailable = false;
            aCommand.model.contentData.status = "Rejected";
            aCommand._onModelChange()
            expect(aCommand.isAvailable).to.be.true;

            aCommand.isAvailable = false;
            aCommand.model.contentData.status = "Published";
            aCommand._onModelChange();
            expect(aCommand.isAvailable).to.be.true;

            aCommand.isAvailable = false;
            aCommand.model.contentData.status = "Expired";
            aCommand._onModelChange();
            expect(aCommand.isAvailable).to.be.true;
        });

        it("is not viewable when the content is in other states than draft", function () {
            var aCommand = new AddTestCommand();
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "Published",
                    isCommonDraft: false,
                    publishedBy: "Timmy",
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand.isAvailable = true;
            aCommand._onModelChange()
            expect(aCommand.isAvailable).to.be.false;

            aCommand.isAvailable = true;
            aCommand.model.contentData.status = "Expired";
            aCommand._onModelChange();
            expect(aCommand.isAvailable).to.be.false;
        });

        it("is executable when available and the user has edit access", function () {
            var aCommand = new AddTestCommand(), aHasAccessFlag = true;
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "CheckedOut",
                    isCommonDraft: false,
                    publishedBy: "Timmy",
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand._contentActionSupport.hasAccess = function (accessMask, accessLevel) {
                return aHasAccessFlag;
            }

            aCommand._onModelChange();
            expect(aCommand.canExecute).to.be.true;
        });

        it("is not executable when not available", function () {
            var aCommand = new AddTestCommand(), aHasAccessFlag = true;
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "Published",
                    isCommonDraft: false,
                    publishedBy: "Timmy",
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand._contentActionSupport.hasAccess = function (accessMask, accessLevel) {
                return aHasAccessFlag;
            }

            aCommand._onModelChange();
            expect(aCommand.canExecute).to.be.false;
        });

        it("is not executable when the user doesn't have edit access", function () {
            var aCommand = new AddTestCommand(), aHasAccessFlag = false;
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "CheckedOut",
                    isCommonDraft: false,
                    publishedBy: "Timmy",
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand._contentActionSupport.hasAccess = function (accessMask, accessLevel) {
                return aHasAccessFlag;
            }

            aCommand._onModelChange();
            expect(aCommand.canExecute).to.be.false;
        });

        it("is not executable when there is no publish version", function () {
            var aCommand = new AddTestCommand(), aHasAccessFlag = true;
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "CheckedOut",
                    isCommonDraft: false,
                    publishedBy: null
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand._contentActionSupport.hasAccess = function (accessMask, accessLevel) {
                return aHasAccessFlag;
            }

            aCommand._onModelChange();
            expect(aCommand.canExecute).to.be.false;
        });

        it("switches to the AddTestView when executed", function () {
            var me = this, aCommand = new AddTestCommand();
            aCommand._topic = {
                publish: function (action, view, data) {
                    me.retAction = action;
                    me.retView = view;
                    me.retData = data;
                }
            }
            aCommand.model = {
                contentData: {
                    title: "ContentTitle"
                },
                languageContext: {}
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: undefined
                    }
                    callBack(activeTest);
                }
            }

            aCommand._execute()

            expect(me.retView).to.equal('AddTestView');
            expect(me.retAction).to.equal('/epi/shell/action/changeview');
            expect(me.retData.contentData).to.deep.equal(aCommand.model.contentData);
        });

        it("is not visible or executable when there is an active test against the content", function () {
            var aCommand = new AddTestCommand()
            aCommand._contentActionSupport = contentActionSupport;
            aCommand.model = {
                contentData: {
                    status: "CheckedOut",
                    isCommonDraft: true,
                    publishedBy: "Timmy",
                }
            };

            aCommand.store = {
                get: function (contentGuid) {
                    return this;
                },
                then: function (callBack) {
                    var activeTest = {
                        title: "We Got One!"
                    }
                    callBack(activeTest);
                }
            }

            aCommand._onModelChange()
            expect(aCommand.isAvailable).to.be.false;
            expect(aCommand.canExecute).to.be.false;
        })
    });
});