﻿define([
  "dojo/_base/declare",
    'epi/dependency',
    'dojo/dom',
    'dojo/dom-style',
    "dojo/topic",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!marketing-testing/views/Details.html",
    'epi/i18n!marketing-testing/nls/abtesting',
    "epi/datetime",
    "epi/username",
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
    datetime,
    username

) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin],
    {
        templateString: template,
        resources: resources,
        contextHistory: null,


        constructor: function () {
            var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
            me.context = contextService.currentContext;
            me.subscribe("/epi/shell/context/changed", me._contextChanged);
        },

        postCreate: function () {
            this._renderData();
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type != 'epi.marketing.testing') {
                return;
            }
            me.context = newContext;
            me._renderData();
        },
        _onPickWinnerOptionClicked: function () {
            var me = this;
            me.contextParameters = { uri: "epi.marketing.testing:///testid=" + this.context.data.test.id + "/PickWinner" };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onAbortOptionClicked: function () {
            var me = this, store = this.store || dependency.resolve("epi.storeregistry").get("marketing.contentTesting");
            store.remove(this.context.data.test.originalItemId);
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _onCancelClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
        },

        _renderData: function () {
            var publishedVariant, draftVariant;

            //Header and Test Start Information
            this.detailsHeaderText.textContent = this.context.data.test.title;

            if (this.context.data.daysElapsed.indexOf("Test") !== -1) {
                this.daysElapsedText.textContent = resources.detailsview.test_not_started_text + " " +
                    resources.detailsview.test_scheduled +
                    datetime.toUserFriendlyString(this.context.data.test.startDate) + " " +
                    resources.detailsview.by + " ";
                this.timeRemainingText.textContent = resources.detailsview.test_not_started_text;
            } else {
                this.daysElapsedText.textContent = resources.detailsview.test_started_label + " " +
                    datetime.toUserFriendlyString(this.context.data.test.startDate) + " " +
                    resources.detailsview.by + " ";
                this.timeRemainingText.textContent = this.context.data.daysRemaining + " " +
                    resources.detailsview.remaining_increment;
            }

            this.testOwner.textContent = this.context.data.test.owner;

            //Published version data
            this.publishedVersionName.textContent = this.context.data.publishedVersionName;
            this.publishedVersionContentLink.textContent = this.context.data.publishedVersionContentLink;
            this.publishedVersionUser.textContent = username.toUserFriendlyString(this.context.data.publishedVersionPublishedBy);
            this.publishedVersionDate.textContent = datetime.toUserFriendlyString(this.context.data.publishedVersionPublishedDate);

            //Draft version data
            this.variantName.textContent = this.context.data.draftVersionName;
            this.variantContentLink.textContent = this.context.data.draftVersionContentLink;
            this.variantUser.textContent = username.toUserFriendlyString(this.context.data.draftVersionChangedBy);
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