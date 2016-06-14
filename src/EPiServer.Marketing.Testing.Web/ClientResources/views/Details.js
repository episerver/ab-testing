define([
  "dojo/_base/declare",
    'epi/dependency',
    'dojo/dom',
    'dojo/dom-style',
    "dojo/topic",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!marketing-testing/views/Details.html",
    'dojo/i18n!marketing-testing/nls/MarketingTestingLabels',
    "epi/datetime",
    'xstyle/css!marketing-testing/css/style.css',
    'xstyle/css!marketing-testing/css/GridForm.css',
    "dijit/form/DropDownButton",
    "dijit/TooltipDialog",
    "dijit/form/Button"

], function (
    declare,
    dependency,
    dom,
    domStyle,
    topic,
    widgetBase,
    templatedMixin,
    widgetsInTemplateMixin,
    template,
    resources,
    datetime

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin],
    {
        templateString: template,
        resources: resources,
        contextHistory: null,


        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;

        },

        postCreate: function () {
            var publishedVariant, draftVariant;
            this.contextHistory = dependency.resolve("epi.cms.BackContextHistory");

            //Header and Test Start Information
            this.detailsHeaderText.textContent = this.context.data.test.title;

            if (this.context.data.daysElapsed.indexOf("Test") !== -1) {
                this.daysElapsedText.textContent = this.context.data.daysElapsed;
                this.timeRemainingText.textContent = this.context.data.daysRemaining;
            } else {
                this.daysElapsedText.textContent = resources.detailsview.test_started_label + ' ' + datetime.toUserFriendlyString(this.context.data.test.startDate) + ' ' + resources.detailsview.by;
                this.ownerText.textContent = this.context.data.test.owner;
                this.timeRemainingText.textContent = this.context.data.daysRemaining;
            }

            //Published version data
            this.publishedVersionName.textContent = this.context.data.publishedVersionName;
            this.publishedVersionContentLink.textContent = this.context.data.publishedVersionContentLink;
            this.publishedVersionUser.textContent = this.context.data.publishedVersionPublishedBy;
            this.publishedVersionDate.textContent = datetime.toUserFriendlyString(this.context.data.publishedVersionPublishedDate);

            //Draft version data
            this.variantName.textContent = this.context.data.draftVersionName;
            this.variantContentLink.textContent = this.context.data.draftVersionContentLink;
            this.variantUser.textContent = this.context.data.draftVersionChangedBy;
            this.variantDate.textContent = datetime.toUserFriendlyString(this.context.data.draftVersionChangedDate);

            //Set the correct corresponding variant data
            if (this.context.data.test.variants[0].itemVersion == this.context.data.publishedVersionContentLink.split('_')[0]) {
                publishedVariant = this.context.data.test.variants[0];
                draftVariant = this.context.data.test.variants[1];
            } else {
                publishedVariant = this.context.data.test.variants[1];
                draftVariant = this.context.data.test.variants[0];
            }

            //Published version views/conversions and meter
            this.publishedVersionConversions.textContent = publishedVariant.conversions;
            this.publishedVersionViews.textContent = publishedVariant.views;
            this.publishedVersionPercentage.textContent = getPercent(publishedVariant.conversions, publishedVariant.views) + "%";
            this.publishedVersionPercentMeter.style.height = getPercent(publishedVariant.conversions, publishedVariant.views) * 1.5 + "px";

            //Draft version views/conversions and meter
            this.variantConversions.textContent = draftVariant.conversions;
            this.variantViews.textContent = draftVariant.views;
            this.variantPercentage.textContent = getPercent(draftVariant.conversions, draftVariant.views) + "%";
            this.variantPercentMeter.style.height = getPercent(draftVariant.conversions, draftVariant.views) * 1.5 + "px";

            //Test description, visitor percentage and total participants
            this.testDescription.textContent = this.context.data.test.description;

            this.visitorPercentageText.textContent = this.context.data.visitorPercentage;
            this.totalParticipantsText.textContent = this.context.data.totalParticipantCount;

            this.contentLinkAnchor.href = this.context.data.conversionLink;
            this.contentLinkAnchor.textContent = this.context.data.conversionContentName;
        },

        _onPickWinnerOptionClicked: function () {
            var contextParameters = { uri: "epi.marketing.testing:///testid=" + this.context.data.test.id + "/PickWinner" };
            topic.publish("/epi/shell/context/request", contextParameters);
        },

        _onAbortOptionClicked: function () {
            var store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");
            store.remove(this.context.data.test.originalItemId);
            this.contextHistory.closeAndNavigateBack(this);
        },

        _onCancelClick: function () {
            this.contextHistory.closeAndNavigateBack(this);
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