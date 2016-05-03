define([
    "dojo/_base/declare",
    'epi/dependency',
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport",
],

function (declare, dependency, _ContentCommandBase, ContentActionSupport) {

    return declare([_ContentCommandBase], {

        name: "Cancel Test",
        label: "Cancel AB Test and edit page",
        tooltip: "Cancel AB test to edit",
        iconClass: "", //Define your own icon css class here.

        _contentActionSupport: ContentActionSupport,

        _execute: function () {
            var me = this,
                store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");

            store.remove(me.model.contentData.contentGuid);
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