define([
'dojo/_base/declare',
 'epi/dependency',
 'dojo/topic',
 'dijit/_WidgetBase',
 'dijit/_TemplatedMixin',
 'dijit/_WidgetsInTemplateMixin',
 'dojo/text!marketing-testing/views/PickWinner.html',
 'epi/i18n!marketing-testing/nls/abtesting',
 'epi/datetime',
 'xstyle/css!marketing-testing/css/style.css',
 'xstyle/css!marketing-testing/css/GridForm.css',
 'dijit/form/Button'
], function (
    declare,
    dependency,
    topic,
    _WidgetBase,
    _TemplatedMixin,
    widgetsInTemplateMixin,
    template,
    resources,
    datetime

) {
    return declare([_WidgetBase, _TemplatedMixin, widgetsInTemplateMixin],
    {
        templateString: template,
        resources: resources,
        contextHistory: null,

        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
        },

        postCreate: function () {
            var publishedVariant, draftVariant, publishedConversionPercent, variantConversionPercent;

            this.store = this.store || dependency.resolve("epi.storeregistry").get("marketing.testingResult");
            this.topic = this.topic || topic;

            this.testOwner.textContent = this.context.data.test.owner;
            this.testCompleted.textContent = datetime.toUserFriendlyString(this.context.data.test.endDate);

            //Set the correct corresponding variant data
            if (this.context.data.test.variants[0].itemVersion == this.context.data.publishedVersionContentLink.split('_')[0]) {
                publishedVariant = this.context.data.test.variants[0];
                draftVariant = this.context.data.test.variants[1];
            } else {
                publishedVariant = this.context.data.test.variants[1];
                draftVariant = this.context.data.test.variants[0];
            }

            this.contextHistory = dependency.resolve("epi.cms.BackContextHistory");

            publishedConversionPercent = getPercent(publishedVariant.conversions, publishedVariant.views);
            variantConversionPercent = getPercent(draftVariant.conversions, draftVariant.views);

            if (publishedConversionPercent === variantConversionPercent) {
                //No change to styling
            }
            else if (publishedConversionPercent > variantConversionPercent) {
                this.publishedDiv.style.height = "20em";
                this.publishedDiv.style.width = "20em";
            } else {
                this.variantDiv.style.height = "20em";
                this.variantDiv.style.width = "20em";
            }

            this.testTitle.textContent = this.context.data.test.title;

            //Published version data
            this.publishedVersionName.textContent = this.context.data.publishedVersionName;
            this.publishedVersionContentLink.textContent = this.context.data.publishedVersionContentLink;
            this.publishedByUser.textContent = this.context.data.publishedVersionPublishedBy;
            this.publishedDate.textContent = datetime.toUserFriendlyString(this.context.data.publishedVersionPublishedDate);
            this.publishedConversionPercent.textContent = publishedConversionPercent + "%";
            //Draft version data
            this.variantName.textContent = this.context.data.draftVersionName;
            this.variantContentLink.textContent = this.context.data.draftVersionContentLink;
            this.editedByUser.textContent = this.context.data.draftVersionChangedBy;
            this.savedDate.textContent = datetime.toUserFriendlyString(this.context.data.draftVersionChangedDate);
            this.variantConversionPercent.textContent = variantConversionPercent + "%";

            //Test information and stats
            this.descriptionText.textContent = this.context.data.test.description;
            this.participationPercent.textContent = this.context.data.visitorPercentage + "%";
            this.totalParticipants.textContent = this.context.data.totalParticipantCount;
            this.testDuration.textContent = this.context.data.daysElapsed;

            //Conversion content
            this.contentLinkAnchor.href = this.context.data.conversionLink;
            this.contentLinkAnchor.textContent = this.context.data.conversionContentName;

        },

        _onCloseButtonClick: function () {
            this.contextHistory.closeAndNavigateBack(this);
        },

        _onPublishedVersionClick: function () {
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.publishedVersionContentLink,
                testId: this.context.data.test.id
            }).then(function (data) {
                var contextParameters = { uri: "epi.cms.contentdata:///" + data };
                topic.publish("/epi/shell/context/request", contextParameters);
            }).otherwise(function () {
                alert("Error Processing Winner: Unable to process and save selected version");
                console.log("Error occurred while processing winning content");
            });
        },

        _onVariantVersionClick: function () {
            this.store.put({
                publishedContentLink: this.context.data.publishedVersionContentLink,
                draftContentLink: this.context.data.draftVersionContentLink,
                winningContentLink: this.context.data.draftVersionContentLink,
                testId: this.context.data.test.id
            }).then(function (data) {
                var contextParameters = { uri: "epi.cms.contentdata:///" + data };
                topic.publish("/epi/shell/context/request", contextParameters);
            }).otherwise(function () {
                alert("Error Processing Winner: Unable to process and save selected version");
                console.log("Error occurred while processing winning content");
            });
        }

    });

    function getPercent(visitors, conversions) {
        if (conversions === 0) {
            return 0;
        }
        var percent = (visitors / conversions) * 100;
        return Math.round(percent);
    }
});