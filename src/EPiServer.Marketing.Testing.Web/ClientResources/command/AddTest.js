define([
    "dojo/_base/declare",
    "dojo/topic",
    "epi/i18n!marketing-testing/nls/abtesting",
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport",
    'epi/dependency'
],

function (declare, topic, resources, _ContentCommandBase, ContentActionSupport, dependency) {

    return declare([_ContentCommandBase], {

        resources: resources,

        name: "AddTest",
        label: resources.addtestcommand.label_text,
        tooltip: resources.addtestcommand.tooltip_text,
        iconClass: "epi-iconClock",

        _contentActionSupport: ContentActionSupport,
        _topic: topic,
        
        _execute: function () {
            var me = this;
            //if not part of a test
            me._topic.publish("/epi/shell/action/changeview", "AddTestView", { contentData: me.model.contentData, languageContext: me.model.languageContext });

            //if part of active test
            // change to active test context
            // 

            //if part of completed test
            // show test resuls view
        },

        _onModelChange: function () {
            var me = this,
                store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");

            store.get(me.model.contentData.contentGuid).then(function (test) {
                me._setVisibility(test, me);
            });
        },

        _getCanExecute: function (contentData, versionStatus, me) {
            return contentData.publishedBy !== null && contentData.publishedBy !== undefined && // This condition indicates that the content has published version.
                contentData.status !== versionStatus.Published &&
                contentData.status !== versionStatus.Expired &&     // Expired content is basically Published content with a expireDate set to the past
                me._contentActionSupport.hasAccess(contentData.accessMask, me._contentActionSupport.accessLevel.Publish); // Ensure has delete action to the user
        },

        _setVisibility: function (test, me) {
            var activeTest = (test.title != undefined && test.title != null),
                contentData = me.model.contentData,
                status = contentData.status,
                versionStatus = me._contentActionSupport.versionStatus;

            var isAvailable = ((status === versionStatus.CheckedOut) ||
                (status === versionStatus.Rejected) ||
                ((status === versionStatus.Published || status === versionStatus.Expired) && contentData.isCommonDraft)) &&
                !activeTest;
            me.set("isAvailable", isAvailable);
            me._setCanExecute(me);
        },

        _setCanExecute: function (me) {
            var contentData = me.model.contentData,
                status = contentData.status,
                versionStatus = me._contentActionSupport.versionStatus;

            //Executable when available and not published, have published version and have edit access right
            me.set("canExecute", me.get("isAvailable") && me._getCanExecute(contentData, versionStatus, me));
            if (me.get("canExecute")) {
                // only update the state for content that can be tested.
                me._topic.publish("/epi/marketing/updatestate", "AddTestView", { contentData: contentData, languageContext: me.model.languageContext });
            }
        }
    });
});