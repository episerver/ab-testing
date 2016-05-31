define([
    "dojo/_base/declare",
    "dojo/_base/event",
    "dojo/dom-construct",
    "dojo/on",
    "dojo/topic",
    "dijit/Destroyable",
    "epi/datetime",
    "epi-cms/contentediting/_ContentEditingNotification"
],

function (
    declare,
    event,
    domConstruct,
    on,
    topic,
    Destroyable,
    datetime,
    _ContentEditingNotification
) {

    return declare([_ContentEditingNotification, Destroyable], {
        // summary:
        //      Shows the notification when active or finished test exists for the current content.
        // tags:
        //      public

        constructor: function (params) {
        },

        postscript: function () {
            this._storeName = "marketing.contentTesting";
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
            //      Set notification(s) when executed success and the test is active or finished
            // tags:
            //      protected

            // Show notification for not started, active and finished tests
            if (!test || !test.title || test.state == 3 || !test.id) {
                this._setNotification(null);
                return;
            }
            
            var notificationMessage = this._constructNotificationMessage(test);

            this._setNotification({
                content: notificationMessage
            });
        },
        
        _constructNotificationMessage: function (test) {
            // TODO: use localized resources.
            var message = "This page is part of a running A/B Test. ";
            var testLinkText = "View test";
            var testLinkTooltip = "View test details";
            
             // Inactive (scheduled)
            if (test.state == 0) {
                message = "An A/B Test is scheduled to run on this page " + 
                    datetime.toUserFriendlyString(test.startDate, null, false, true) +". ";
            }
            
             // Done, finished
            if (test.state == 2) {
                message = "An A/B Test has been completed on this page. ";
                var testLinkText = "Pick winner";
                var testLinkTooltip = "View test details and pick winner";
            }

            var notificationMesage = domConstruct.create("div", {innerHTML: message});
            
            var testLink =  domConstruct.create("a", { href: "#", innerHTML: testLinkText, title: testLinkTooltip }, notificationMesage);
            
            this.own(
                on(testLink, "click", function (e) {
                    event.stop(e);
                    topic.publish("/epi/shell/context/request", {
                        uri: "epi.marketing.testing:///" + test.id
                    }, {});
                })
            );
            
            return notificationMesage;
        }
    });

});