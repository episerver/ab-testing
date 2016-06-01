define([
  'dojo/_base/declare',
     'epi/dependency',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    "dijit/_WidgetsInTemplateMixin",
    'dojo/text!marketing-testing/views/PickWinner.html',
    'dojo/i18n!marketing-testing/nls/MarketingTestingLabels',
    "epi/datetime",
    'xstyle/css!marketing-testing/css/style.css',
    'xstyle/css!marketing-testing/css/GridForm.css',
    "dijit/form/Button"
], function (
    declare,
    dependency,
    _WidgetBase,
    _TemplatedMixin,
    widgetsInTemplateMixin,
    template,
    resources,
    datetime

) {
    return declare([_WidgetBase, _TemplatedMixin, widgetsInTemplateMixin], {
        templateString: template,
        resources: resources,

        contextHistory: null,

        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
        },

        postCreate: function () {
            var publishedVariant, draftVariant, publishedConversionPercent, variantConversionPercent;


            this.testOwner.textContent = this.context.data.test.owner;
            this.testCompleted.textContent = datetime.toUserFriendlyString(this.context.data.test.endDate);

            //Set the correct corresponding variant data
            if (this.context.data.test.variants[0].itemversion === this.context.data.publishedVersionContentLink) {
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
                this.publishedDiv.style.widthidth = "20em";
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

            this.descriptionText.textContent = this.context.data.test.description;
            this.participationPercent.textContent = this.context.data.visitorPercentage + "%";
            this.totalParticipants.textContent = this.context.data.totalParticipantCount;
            this.testDuration.textContent = this.context.data.daysElapsed;

            this.contentLinkAnchor.href = this.context.data.conversionLink;
            this.contentLinkAnchor.textContent = this.context.data.conversionContentName;

        },

        _onCloseButtonClick: function () {
            this.contextHistory.closeAndNavigateBack(this);
        },

        _onPublishedVersionClick: function () {
            alert("PUBLISHED VERSION CLICKED");
            //WIRE IN WHAT TO DO FOR PUBLISEHD VERISION SELECTION
            //Leave as is? Set test to "archived"?
        },

        _onVariantVersionClick: function () {
            alert("VARIANT VERSION CLICKED");
            //WIRE IN WHAT TO DO FOR VARIANT VERSION SELECTION
            //Publishe variant? Set test to archved?"
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