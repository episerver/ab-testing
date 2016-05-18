define([
    "dojo/_base/declare",
    'epi/dependency',
    'dojo/dom',
    'dojo/dom-style',
    "dojo/topic",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!marketing-testing/views/MarketingTestDetailsView.html",
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

        constructor: function () {
            var contextService = epi.dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
        },

        postCreate: function () {
            var publishedVariant, draftVariant;
            this.DetailsHeader.textContent = this.context.data.test.title;

            if (this.context.data.daysElapsed.indexOf("Test") !== -1) {
                this.daysElapsed.textContent = this.context.data.daysElapsed;
                this.timeRemaining.textContent = this.context.data.daysRemaining;
            } else {
                this.daysElapsed.textContent = resources.detailsview.test_started_label + ' ' + datetime.toUserFriendlyString(this.context.data.test.startDate) + ' ' + resources.detailsview.by;
                this.Owner.textContent = this.context.data.test.owner;
                this.timeRemaining.textContent = this.context.data.daysRemaining;
            }

            this.VersionAText.textContent = this.context.data.publishedVersionName;
            this.VersionAContentLink.textContent = this.context.data.publishedVersionContentLink;
            this.PublishedUser.textContent = this.context.data.publishedVersionPublishedBy;
            this.PublishedDate.textContent = datetime.toUserFriendlyString(this.context.data.publishedVersionPublishedDate);

            this.VersionBText.textContent = this.context.data.draftVersionName;
            this.VersionBContentLink.textContent = this.context.data.draftVersionContentLink;
            this.DraftUser.textContent = this.context.data.draftVersionChangedBy;
            this.DraftSavedDate.textContent = datetime.toUserFriendlyString(this.context.data.draftVersionChangedDate);


            if (this.context.data.test.variants[0].itemversion === this.context.data.publishedVersionContentLink) {
                publishedVariant = this.context.data.test.variants[0];
                draftVariant = this.context.data.test.variants[1];
            } else {
                publishedVariant = this.context.data.test.variants[1];
                draftVariant = this.context.data.test.variants[0];
            }

            this.firstVariantConversions.textContent = publishedVariant.conversions;
            this.firstVariantViews.textContent = publishedVariant.views;
            this.firstVariantPercentage.textContent = getPercent(publishedVariant.conversions, publishedVariant.views) + "%";
            this.firstVariantPercentMeter.style.height = getPercent(publishedVariant.conversions, publishedVariant.views) * 1.5 + "px";

            this.secondtVariantConversions.textContent = draftVariant.conversions;
            this.secondVariantViews.textContent = draftVariant.views;
            this.secondVariantPercentage.textContent = getPercent(draftVariant.conversions, draftVariant.views) + "%";
            this.secondVariantPercentMeter.style.height = getPercent(draftVariant.conversions, draftVariant.views) * 1.5 + "px";

            this.TestDescription.textContent = this.context.data.test.description;

            this.visitorPercentage.textContent = this.context.data.visitorPercentage;
            this.totalParticipants.textContent = this.context.data.totalParticipantCount;
            //   this.firstVariantPercentMeter.height = "height:" + 300/getPercent(10, 100) + " px";

        },

        _onWinnerOptionClicked: function () {
            var me = this;
            var contextParameters = { uri: "epi.marketing.testing:///testid=" + this.context.data.test.id + "/MarketingTestPickWinnerView" };
            topic.publish("/epi/shell/context/request", contextParameters);
        },

        _onAbortOptionClicked: function () {
            var store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");
            store.remove(this.context.data.test.originalItemId);
            topic.publish("/epi/shell/action/changeview/back");
        },

        _onCancelClick: function () {
            topic.publish("/epi/shell/action/changeview/back");
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