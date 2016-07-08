define([
    "dojo/_base/declare",
    "dojo/_base/event",
    "dojo/Stateful",
    "dojo/dom-construct",
    "dojo/on",
    "dojo/topic",
    "dijit/Destroyable",
    "epi/datetime",
    'epi/dependency',
    "epi/i18n!marketing-testing/nls/abtesting"
],

function (
    declare,
    event,
    Stateful,
    domConstruct,
    on,
    topic,
    Destroyable,
    datetime,
    dependency,
    resources
) {

    return declare([Stateful, Destroyable], {
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

        _valueSetter: function (value) {
            var me = this;
            this.value = value;
            this._store = this._store || dependency.resolve("epi.storeregistry").get(this._storeName);

            this._store.get(value.contentData.contentGuid).then(function (test) {
                // Hide notification for content without a pending or active test
                if (!test || !test.title || test.state == 3 || !test.id) {
                    me.set("notification", null);
                    return;
                }

                var notificationMessage = me._constructNotificationMessage(test);
                me.set("notification", {
                    content: notificationMessage,
                    commands: []
                });
            });
        },

        _constructNotificationMessage: function (test) {
            var message = resources.notificationbar.ongoing_test;
            var testLinkText = resources.notificationbar.details_link_text;
            var testLinkTooltip = resources.notificationbar.details_link_tooltip;
            var page = "details";

            // Inactive (scheduled)
            if (test.state == 0) {
                message = resources.notificationbar.scheduled_test +
                    datetime.toUserFriendlyString(test.startDate, null, false, true) + ". ";
            }

            // Done, finished
            if (test.state == 2) {
                message = resources.notificationbar.completed_test;
                var testLinkText = resources.notificationbar.winner_link_text;
                var testLinkTooltip = resources.notificationbar.winner_link_tooltip;
                page = "pickwinner";
            }

            var notificationMesage = domConstruct.create("div", { innerHTML: message });

            var testLink = domConstruct.create("a", { href: "#", innerHTML: testLinkText, title: testLinkTooltip }, notificationMesage);

            this.own(
                on(testLink, "click", function (e) {
                    event.stop(e);
                    topic.publish("/epi/shell/context/request", {
                        uri: "epi.marketing.testing:///testid=" + test.id + "/" + page
                    }, {sender: this});
                })
            );
            
            return notificationMesage;
        }
    });

});