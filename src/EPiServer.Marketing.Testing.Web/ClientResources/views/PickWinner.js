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

        _onCloseButtonClick: function () {
            var me = this;
            me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink.split('_')[0] };
            topic.publish("/epi/shell/context/request", me.contextParameters);
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
        },

        _renderData: function () {
            var publishedVariant, draftVariant, publishedConversionPercent, variantConversionPercent;

            //reset the div sizes for the versions to prevent errors viewing multiple tests
            this._resetVersionDivs();

            this.store = dependency.resolve("epi.storeregistry").get("marketing.testingResult");
            this.topic = this.topic || topic;

            if (this.context.data.test.state === 0) {
                this.scheduleStatusText.innerHTML =
                    resources.pickwinnerview.test_not_started + "&nbsp" +
                    resources.pickwinnerview.test_scheduled +
                    datetime.toUserFriendlyString(this.context.data.test.startDate) + ".";
            } else if (this.context.data.test.state === 1) {
                this.scheduleStatusText.innerHTML =
                    resources.pickwinnerview.started_by_text +
                    "<span class='epi-username'>" + this.context.data.test.owner + "</span>" + ".&nbsp" +
                    resources.pickwinnerview.test_scheduled_finish +
                    datetime.toUserFriendlyString(this.context.data.test.endDate) + ".";
            } else {
                this.scheduleStatusText.innerHTML =
                    resources.pickwinnerview.started_by_text +
                    "<span class='epi-username'>" + this.context.data.test.owner + "</span>" + ".&nbsp" +
                    resources.pickwinnerview.completed_text +
                    datetime.toUserFriendlyString(this.context.data.test.endDate) + ".";
            }

            //Set the correct corresponding variant data
            if (this.context.data.test.variants[0].itemVersion == this.context.data.publishedVersionContentLink.split('_')[0]) {
                publishedVariant = this.context.data.test.variants[0];
                draftVariant = this.context.data.test.variants[1];
            } else {
                publishedVariant = this.context.data.test.variants[1];
                draftVariant = this.context.data.test.variants[0];
            }

            publishedConversionPercent = getPercent(publishedVariant.conversions, publishedVariant.views);
            variantConversionPercent = getPercent(draftVariant.conversions, draftVariant.views);

            this._setVersionDivs(publishedConversionPercent, variantConversionPercent);

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

            if (this.context.data.test.state === 0) {
                this.testDuration.textContent = this.context.data.daysElapsed;
                this.durationText.textContent = "";
            } else {
                this.testDuration.textContent = this.context.data.daysElapsed;
                this.durationText.textContent = resources.pickwinnerview.duration_text;
            }

            //Conversion content
            this.contentLinkAnchor.href = this.context.data.conversionLink;
            this.contentLinkAnchor.textContent = this.context.data.conversionContentName;
        },

        _resetVersionDivs: function () {
            this.publishedDiv.style.height = "auto";
            this.publishedDiv.style.width = "auto";
            this.variantDiv.style.height = "auto";
            this.variantDiv.style.width = "auto";
        },

        _setVersionDivs: function (publishedConversionPercent, variantConversionPercent) {
            if (publishedConversionPercent == variantConversionPercent) {
                //do not change the default div dimensions.
            }
            else if (publishedConversionPercent > variantConversionPercent) {
                this.publishedDiv.style.height = "20em";
                this.publishedDiv.style.width = "20em";
            } else {
                this.variantDiv.style.height = "20em";
                this.variantDiv.style.width = "20em";
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