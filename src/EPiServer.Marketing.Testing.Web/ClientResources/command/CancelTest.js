define([
    "dojo/_base/declare",
    "dojo/topic",
    'epi/dependency',
    "epi/i18n!marketing-testing/nls/abtesting",
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport"
],

function (declare, topic, dependency, resources, _ContentCommandBase, ContentActionSupport) {

    return declare([_ContentCommandBase], {
        resources: resources,

        name: "Cancel Test",
        label: resources.canceltestcommand.label_text,
        tooltip:resources.canceltestcommand.tooltip_text,
        iconClass: "epi-iconPen", //Define your own icon css class here.

        _contentActionSupport: ContentActionSupport,

        _execute: function () {
            var me = this,
                store = this.store || dependency.resolve("epi.storeregistry").get("marketing.abtesting");

            store.remove(me.model.contentData.contentGuid).then(function () {
                var contextParameters = { uri: "epi.cms.contentdata:///" + me.model.contentData.contentLink };
                topic.publish("/epi/shell/context/request", contextParameters);
            });
        },

        _onModelChange: function () {
            var me = this,
                store = this.store || dependency.resolve("epi.storeregistry").get("marketing.abtesting"),
                menu = this.menu || dependency.resolve("epi.globalcommandregistry").get("epi.cms.publishmenu"),
                contentData = me.model.contentData;

            //call the rest store to see if there is a test associated with the content being looked at
            //set isAvailable and canExecute to true when there is a test set up and the test is not completed(2) or archived(3)
            //disable the other publish options
            store.get(contentData.contentGuid).then(function (data) {
                var isVisible = false, isClickable = false;
                if (data.title != undefined && data.title != null && data.state != 3) {
                    isVisible = true;

                    if (me._contentActionSupport.hasAccess(contentData.accessMask, me._contentActionSupport.accessLevel.Publish)) {
                        isClickable = true;
                    }
                }

                if (isVisible) {
                    for (var i = 0; i < menu.providers.length; i++) {
                        for (var j = 0; j < menu.providers[i].commands.length; j++) {
                            menu.providers[i].commands[j].set("isAvailable", false);
                        }
                    }
                }

                me.set("isAvailable", isVisible);
                me.set("canExecute", isClickable);
            });
        }
    });
});