define([
    // General application modules
    "dojo/_base/declare",
    "dojo/_base/lang",
    "dojo/topic",
    "dojo/when",
    "epi",
    "epi/dependency",
    // Parent class
    "epi-cms/contentediting/command/_ContentAreaCommand",
    "epi-cms/contentediting/ContentActionSupport",
        "epi/i18n!epi/cms/nls/episerver.shared.action"
], function (declare, lang, topic, when, epi, dependency, _ContentAreaCommand, ContentActionSupport, actionStrings) {

    return declare([_ContentAreaCommand], {
        // summary:
        //      View test command
        // tags:
        //      public

        constructor: function () {
            var registry = dependency.resolve("epi.storeregistry");
            this._store = registry.get("marketing.testing");
        },

        _execute: function () {
            // summary:
            //      Change the context to edit the block.
            // tags:
            //      protected
            var testId = this.model.testId;
            topic.publish("/epi/shell/context/request", {
                uri: "epi.marketing.testing:///" + testId
            }, {});
        },

        _onModelValueChange: function () {
            // summary:
            //      Updates canExecute after the model value has changed.
            // tags:
            //      protected
            var item = this.model;

            if (!item || !item.testId) {
                this.set("canExecute", false);
                return;
            }

            var result = item && this._store.get(item.testId);

            // if the accessMask is available then display the label accordingly. (i.e either "View" or "Edit")
            when(result, lang.hitch(this, function (test) {
                this.set("canExecute", test && true);
            }));
        }
    });
});
