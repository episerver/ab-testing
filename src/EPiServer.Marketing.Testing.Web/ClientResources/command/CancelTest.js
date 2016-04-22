define([
    "dojo/_base/declare",
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport",
],

function (declare, _ContentCommandBase, ContentActionSupport) {

    return declare([_ContentCommandBase], {

        name: "Cancel Test",
        label: "Cancel AB Test and edit page",
        tooltip: "Cancel AB test to edit",
        iconClass: "", //Define your own icon css class here.

        _contentActionSupport: ContentActionSupport,

        postscript: function () {
            this.store = this.store || dependency.resolve("epi.storeregistry").get("marketing.testing");
        },

        _execute: function () {
            var me = this;

            
        },

        _onModelChange: function () {
            var me = this
            //call the rest store to see if there is a test associated with the content being looked at
            //set isAvailable and canExecute to true when there is a test set up
            //disable the other publish options

            this.set("isAvailable", false);
            this.set("canExecute", false);
        }
    });
});