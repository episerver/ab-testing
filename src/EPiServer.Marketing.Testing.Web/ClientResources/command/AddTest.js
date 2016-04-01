define([
    "dojo/_base/declare",
    "epi/dependency",
    "dojo/topic",
    "epi-cms/contentediting/command/_ContentCommandBase",
    "epi-cms/contentediting/ContentActionSupport",
],

function (declare, dependcy, topic, _ContentCommandBase, ContentActionSupport) {

    return declare([_ContentCommandBase], {

        name: "AddTest",
        label: "Create AB Test",
        tooltip: "Create AB test from my changes",
        iconClass: "", //Define your own icon css class here.

        _execute: function () {
            //if not part of a test
            topic.publish("/epi/shell/action/changeview", "AddTestView", { contentData: this.model.contentData, languageContext: this.model.languageContext });

            //if part of active test
            // change to active test context
            // 

            //if part of completed test
            // show test resuls view
        },

        _onModelChange: function () {
            var contentData = this.model.contentData,
                status = contentData.status,
                versionStatus = ContentActionSupport.versionStatus;

            //check content for tests
           


            //Available when content is a draft
            var isAvailable = (status === versionStatus.CheckedOut) ||
                (status === versionStatus.Rejected) ||
                ((status === versionStatus.Published || status === versionStatus.Expired) && contentData.isCommonDraft);
            this.set("isAvailable", isAvailable);

            //Executable when available and not published, have published version and have edit access right
            this.set("canExecute", isAvailable && this._getCanExecute(contentData, versionStatus));
        },

        _getCanExecute: function (contentData, versionStatus) {
            return contentData.publishedBy !== null && contentData.publishedBy !== undefined && // This condition indicates that the content has published version.
                contentData.status !== versionStatus.Published &&
                contentData.status !== versionStatus.Expired &&     // Expired content is basically Published content with a expireDate set to the past
                ContentActionSupport.hasAccess(contentData.accessMask, ContentActionSupport.accessLevel.Publish); // Ensure has delete action to the user
        }
    });
});