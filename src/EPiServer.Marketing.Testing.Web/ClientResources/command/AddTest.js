define([
    "dojo/_base/declare",
    "dojo/topic",
    "dojo/i18n!marketing-testing/nls/MarketingTestingLabels",
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport",
],

function (declare, topic, _ContentCommandBase, ContentActionSupport) {

    return declare([_ContentCommandBase], {

        resources: resources,

        name: "AddTest",
        label: resources.addtestcommand.label_text,
        tooltip: resources.addtestcommand.tooltip_text,
        iconClass: "", //Define your own icon css class here.

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
            var me = this, contentData = me.model.contentData,
                status = contentData.status,
                versionStatus = me._contentActionSupport.versionStatus;

            //Available when content is a draft
            var isAvailable = (status === versionStatus.CheckedOut) ||
                (status === versionStatus.Rejected) ||
                ((status === versionStatus.Published || status === versionStatus.Expired) && contentData.isCommonDraft);
            this.set("isAvailable", isAvailable);

            //Executable when available and not published, have published version and have edit access right
            this.set("canExecute", isAvailable && this._getCanExecute(contentData, versionStatus));
        },

        _getCanExecute: function (contentData, versionStatus) {
            var me = this;
            return contentData.publishedBy !== null && contentData.publishedBy !== undefined && // This condition indicates that the content has published version.
                contentData.status !== versionStatus.Published &&
                contentData.status !== versionStatus.Expired &&     // Expired content is basically Published content with a expireDate set to the past
                me._contentActionSupport.hasAccess(contentData.accessMask, me._contentActionSupport.accessLevel.Publish); // Ensure has delete action to the user
        }
    });
});