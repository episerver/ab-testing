define([
// dojo
    "dojo/_base/array",
    "dojo/_base/declare",
    "dojo/_base/event",
    "dojo/dom-construct",
    "dojo/on",
    "dojo/when",
// epi
    "epi",
    "epi/shell/TypeDescriptorManager",
    "epi-cms/contentediting/_ContentEditingNotification",
    "epi-cms/command/BackCommand",
    "epi-cms/widget/sharedContentDialogHandler",
    'marketing-testing/command/ViewTestCommand',
    'marketing-testing/viewmodels/TestModel'
],

function (
// dojo
    array,
    declare,
    event,
    domConstruct,
    on,
    when,
// epi
    epi,
    TypeDescriptorManager,
    _ContentEditingNotification,
    BackCommand,
    sharedContentDialogHandler,
    ViewTestCommand,
    TestModel
) {

    return declare([_ContentEditingNotification], {
        // tags:
        //      public

        _viewTestCommand: null,

        constructor: function (params) {

            this._viewTestCommand = new ViewTestCommand();
        },

        postscript: function () {
            this._storeName = "marketing.contenttesting";
            this.inherited(arguments);
        },

        _executeAction: function (/*Object*/value) {
            // summary:
            //      Get active test
            // tags:
            //      protected, extension

            return this._store.get(value.contentData.contentGuid);
        },

        _onExecuteSuccess: function (/*Object*/test) {
            // summary:
            //      Set notification(s) when executed success
            // tags:
            //      protected

            // Show notification for active and finished tests only
            if (!test || test.State == 0 || test.State == 3 || !test.Id) {
                this._setNotification(null);
                return;
            }
            
            var notificationMessage = this._constructNotificationMessage(test);
            var model = new TestModel(test);            

            this._viewTestCommand.set("model", model);

            this._setNotification({
                content: notificationMessage,
                commands: [this._viewTestCommand]
            });
        },
        
        _constructNotificationMessage: function (test) {
            // use localized resources
            var message = "This page is part of a running A/B Test";
            var testLinkText = "View test";
            var testLinkTooltip = "View test details";

            var notificationMesage = domConstruct.create("div", {innerHTML: message});
            
            var testLink =  domConstruct.create("a", { href: "#", innerHTML: testLinkText, title: testLinkTooltip }, notificationMesage);
            
            return notificationMesage;
        }
    });

});
