define([
    "dojo/_base/declare",
    "dojo/topic",
    'epi/dependency',
    "dojo/i18n!marketing-testing/nls/MarketingTestingLabels",
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport"
],

function (declare, topic, dependency, resources, _ContentCommandBase, ContentActionSupport) {

    return declare([_ContentCommandBase], {
        resources: resources,

        name: "Cancel Test",
        label: resources.canceltestcommand.label_text,
        tooltip:resources.canceltestcommand.tooltip_text,
        iconClass: "", //Define your own icon css class here.

        _contentActionSupport: ContentActionSupport,

        _execute: function () {
            var me = this,
                store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");

            store.remove(me.model.contentData.contentGuid).then(function () {
                var contentId = me.model.contentData.contentLink.split("_"), contextParameters = { uri: "epi.cms.contentdata:///" + contentId[0] };
                topic.publish("/epi/shell/context/request", contextParameters);
            });
        },

        _onModelChange: function () {
            var me = this
            var store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");
            //call the rest store to see if there is a test associated with the content being looked at
            //set isAvailable and canExecute to true when there is a test set up
            //disable the other publish options
            store.get(me.model.contentData.contentGuid).then(function (data) {
                var isVisible = false, isClickable = false;
                if (data.title != undefined && data.title != null) {
                    isVisible = true;
                    isClickable = true;
                }
                me.set("isAvailable", isVisible);
                me.set("canExecute", isClickable);
            });
        }
    });
});