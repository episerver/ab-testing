function showSettings() {
    $('.advanced-options__content').animate({
        height: "toggle"
    }, "fast");
};

define([
    "dojo/_base/declare",
    "epi/dependency",
    "dojo/Stateful",
    "dojo/topic"
], function (
    declare,
    dependency,
    stateful,
    topic
) {
    return declare([stateful], {

        //First Content version to be used as potential content swap
        //during active A/B test.
        publishedVersion: null,

        //Second content version to be used as potential content swap
        //during active A/B test.
        currentVersion: null,

        //Content ID of the content under test
        testContentId: null,

        //title of the current test
        testTitle: null,

        //description provided by the user.
        testDescription: null,

        //page, which when loaded, triggers a conversion from a page under test.
        conversionPage: null,

        //percentage of visitors to include in the test.
        participationPercent: null,

        //duration, in days, the test should run.
        testDuration: null,

        //start date (currently set to "now" when they hit the start test button
        startDate: null,

        //property to start a test immediately upon creation
        start: true,

        //confidence level
        confidenceLevel: null,

        autoPublishWinner: null,

        postscript: function () {
            this.inherited(arguments);
            this.setupContentData();
            this.store = this.store || dependency.resolve("epi.storeregistry").get("marketing.abtesting");
            this.configStore = this.configStore || dependency.resolve("epi.storeregistry").get("marketing.abtestingconfig");
            this.topic = this.topic || topic;

            this._contextChangedHandler = dojo.subscribe('/epi/marketing/updatestate', this, this._onContextChange);
        },

        _onContextChange: function (context, caller) {
            // Widget will update itself using the new context.
            this.contentData = caller.contentData;
            this.contentData.contentLink = caller.contentData.contentLink;
            this.setupContentData();
        },

        setupContentData: function () {
            //get published version
            this._contentVersionStore = this._contentVersionStore || dependency.resolve("epi.storeregistry").get("epi.cms.contentversion");
            this._contentVersionStore
                .query({ contentLink: this.contentData.contentLink, language: this.languageContext ? this.languageContext.language : "", query: "getpublishedversion" })
                .then(function (result) {
                    var publishedVersion = result;
                    this.set("publishedVersion", publishedVersion);
                    this.set("currentVersion", this.contentData);

                    this.configStore.get()
                        .then(function (config) {
                            console.log(config);
                            this.set("testDuration", config.testDuration);
                            this.set("participationPercent", config.participationPercent);
                            this.set("confidenceLevel", config.confidenceLevel);
                            if (config.autoPublishWinner) {
                                this.set("autoPublishWinner", 1);
                            } else {
                                this.set("autoPublishWinner", 2);
                            }
                        }.bind(this));

                    console.log(result);
                    console.log(this.contentData);
                }.bind(this))
                .otherwise(function () {
                    console.log("Query did not return valid result");
                });
        },

        createTest: function () {
            var published = this.publishedVersion.contentLink.split('_');
            var draft = this.currentVersion.contentLink.split('_');
            var me = this;
            this.store.put({
                testDescription: this.testDescription,
                testContentId: this.contentData.contentGuid,
                publishedVersion: published[1],
                variantVersion: draft[1],
                testDuration: this.testDuration,
                participationPercent: this.participationPercent,
                kpiId: this.kpiId,
                testTitle: this.testTitle,
                startDate: this.startDate,
                start: this.start,
                confidencelevel: this.confidencelevel
            }).then(function () {
                var contextParameters = { uri: "epi.cms.contentdata:///" + me.currentVersion.contentLink };
                me.topic.publish("/epi/shell/context/request", contextParameters);
            }).otherwise(function () {
                console.log("Error occured while creating Marketing Test - Unable to create test");
            });
        }
    });

});