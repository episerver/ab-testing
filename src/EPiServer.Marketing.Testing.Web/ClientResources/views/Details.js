define([
  "dojo/_base/declare",
    'epi/dependency',
    'dojo/dom',
    'dijit/registry',
    'dojo/dom-style',
    "dojo/topic",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!marketing-testing/views/Details.html",
    'epi/i18n!marketing-testing/nls/abtesting',
    "epi/datetime",
    "epi/username",
    "dojo/dom-class",
    'xstyle/css!marketing-testing/css/ABTesting.css',
    'xstyle/css!marketing-testing/css/GridForm.css',
    'xstyle/css!marketing-testing/css/dijit.css',
    "dijit/form/DropDownButton",
    "dijit/TooltipDialog",
    "dijit/form/Button"

], function (
    declare,
    dependency,
    dom,
    registry,
    domStyle,
    topic,
    widgetBase,
    templatedMixin,
    widgetsInTemplateMixin,
    template,
    resources,
    datetime,
    username,
    domClass

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

        startup: function () {
            this._DisplayOptionsButton(this.context.data.userHasPublishRights);
        },

        _contextChanged: function (newContext) {
            var me = this;
            if (!newContext || newContext.type !== 'epi.marketing.testing') {
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
            var me = this, store = this.store || dependency.resolve("epi.storeregistry").get("marketing.abtesting");
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
            this.pageName.textContent = this.context.data.test.title;

            if (this.context.data.test.state === 0) {
                this.headerStateAndElapsedText.textContent = resources.detailsview.test_status_not_started;
                this.headerStartedText.textContent = resources.detailsview.test_scheduled +
                    datetime.toUserFriendlyString(this.context.data.test.startDate);

            } else if (this.context.data.test.state === 1) {
                this.headerStateAndElapsedText
                    .textContent = resources.detailsview.test_status_running + this.context.data.daysRemaining + " " + resources.detailsview.days_remaining;
                this.headerStartedText.textContent = resources.detailsview.started +
                    datetime.toUserFriendlyString(this.context.data.test.startDate) +
                    " " +
                    resources.detailsview.by +
                    " " +
                    username.toUserFriendlyString(this.context.data.test.owner);

            } else if (this.contexxt.data.test.state === 3) {
                this.headerStateAndElapsedText.textContent = resources.detailsview.test_status_completed;
                this.headerStartedText.textContent = "";
            }

            this.testDuration.textContent = this.context.data.daysElapsed;
            this.timeRemaining.textContent = this.context.data.daysRemaining;
            this.selectedConfidenceLevel.textContent = this.context.data.test.confidenceLevel + "%";

            //Published version data
            this.publishedBy.textContent = username.toUserFriendlyString(this.context.data.publishedVersionPublishedBy);
            this.datePublished.textContent = datetime.toUserFriendlyString(this.context.data.publishedVersionPublishedDate);

            //Draft version data
            this.savedBy.textContent = username.toUserFriendlyString(this.context.data.draftVersionChangedBy);
            this.dateSaved.textContent = datetime.toUserFriendlyString(this.context.data.draftVersionChangedDate);

            //Set the correct corresponding variant data
            if (this.context.data.test.variants[0].itemVersion === this.context.data.publishedVersionContentLink.split('_')[0]) {
                publishedVariant = this.context.data.test.variants[0];
                draftVariant = this.context.data.test.variants[1];
            } else {
                publishedVariant = this.context.data.test.variants[1];
                draftVariant = this.context.data.test.variants[0];
            }

            var publishedPercent = getPercent(publishedVariant.conversions, publishedVariant.views);
            var variantPercent = getPercent(draftVariant.conversions, draftVariant.views);

            //Published version views/conversions and meter
            this.publishedVersionConversions.textContent = publishedVariant.conversions;
            this.publishedVersionViews.textContent = publishedVariant.views;
            this.publishedVersionPercentage.textContent = publishedPercent + "%";


            //Draft version views/conversions and meter
            this.variantConversions.textContent = draftVariant.conversions;
            this.variantViews.textContent = draftVariant.views;
            this.variantPercentage.textContent = variantPercent + "%";

            //Test description, visitor percentage and total participants
            this.testDescription.textContent = "\"" +
                this.context.data.test.description +
                "\" - " + username.toUserFriendlyString(this.context.data.test.owner);

            this.visitorPercentageText.textContent = this.context.data.visitorPercentage;
            this.totalParticipantsText.textContent = this.context.data.totalParticipantCount;

            this.contentLinkAnchor.href = this.context.data.conversionLink;
            this.contentLinkAnchor.textContent = this.context.data.conversionContentName;

            //Set Test Result Status Icons and styles
            //Icons are to show on content which is ahead in conversoins
            //Styles change size and color of container to accomodate for border
            //keeping things aligned properly.
            //Styles and icons adjust based on A > B, A < B, A = B, and 0 and 
            //whether test is active or complete.
            var statusIndicatorClass = "noIndicator";

            if (this.context.data.test.state === 1) {
                statusIndicatorClass = "leadingContent";
            }
            else if (this.context.data.test.state > 2) {
                statusIndicatorClass = "winningContent";
            }

            if (publishedPercent > variantPercent) {
                domClass.replace(this.publishedStatusIcon, statusIndicatorClass);
                domClass.replace(this.variantStatusIcon, "noIndicator");
                domStyle.set(this.publishedVersionPercentage, "font-size", "42px");
                domStyle.set(this.variantPercentage, "font-size", "32px");
                domStyle.set(this.controlHeader, "background-color", "#95c532");
                domStyle.set(this.controlWrapper, "border", "3px solid #95c532");
                domStyle.set(this.controlWrapper, "width", "48.6%");
                domStyle.set(this.challengerHeader, "background-color", "#f7542b");
                domStyle.set(this.challengerWrapper, "border", "0px");
                domStyle.set(this.challengerWrapper, "width", "49%");
            }
            else if (publishedPercent < variantPercent) {
                domClass.replace(this.publishedStatusIcon, "noIndicator");
                domClass.replace(this.variantStatusIcon, statusIndicatorClass);
                domStyle.set(this.publishedVersionPercentage, "font-size", "32px");
                domStyle.set(this.variantPercentage, "font-size", "42px");
                domStyle.set(this.controlHeader, "background-color", "#1ba4fa");
                domStyle.set(this.controlWrapper, "border", "0px");
                domStyle.set(this.controlWrapper, "width", "48.6%");
                domStyle.set(this.challengerHeader, "background-color", "#95c532");
                domStyle.set(this.challengerWrapper, "border", "3px solid #95c532");
                domStyle.set(this.challengerWrapper, "width", "49%");
            }
            else if (publishedPercent === 0 && variantPercent === 0) {
                domClass.replace(this.publishedStatusIcon, "noIndicator");
                domClass.replace(this.variantStatusIcon, "noIndicator");
                domStyle.set(this.publishedVersionPercentage, "font-size", "32px");
                domStyle.set(this.variantPercentage, "font-size", "32px");
                domStyle.set(this.controlHeader, "background-color", "#1ba4fa");
                domStyle.set(this.controlWrapper, "border", "0px");
                domStyle.set(this.controlWrapper, "width", "49%");
                domStyle.set(this.challengerHeader, "background-color", "#f7542b");
                domStyle.set(this.challengerWrapper, "border", "0px");
                domStyle.set(this.challengerWrapper, "width", "49%");
            }
            else {
                domClass.replace(this.publishedStatusIcon, statusIndicatorClass);
                domClass.replace(this.variantStatusIcon, statusIndicatorClass);
                domStyle.set(this.publishedVersionPercentage, "font-size", "42px");
                domStyle.set(this.variantPercentage, "font-size", "42px");
                domStyle.set(this.controlHeader, "background-color", "#95c532");
                domStyle.set(this.controlWrapper, "border", "3px solid #95c532");
                domStyle.set(this.controlWrapper, "width", "48.65%");
                domStyle.set(this.challengerHeader, "background-color", "#95c532");
                domStyle.set(this.challengerWrapper, "border", "3px solid #95c532");
                domStyle.set(this.challengerWrapper, "width", "48.65%");
            }
        },

        _DisplayOptionsButton: function (show) {
            var dropDownButton = registry.byId("optionsDropdown");
            if (show) {
                domStyle.set(dropDownButton.domNode, "visibility", "visible");
                dropDownButton.startup(); //Avoids conditions where the widget is rendered but not active.
            } else {
                domStyle.set(dropDownButton.domNode, "visibility", "hidden");
            }
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