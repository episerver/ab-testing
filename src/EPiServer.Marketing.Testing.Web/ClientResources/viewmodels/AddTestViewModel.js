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

        postscript: function () {
            this.inherited(arguments);
            this.setupContentData();
            this.store = this.store || dependency.resolve("epi.storeregistry").get("marketing.testing");
            this.topic = this.topic || topic;
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
                    console.log(result);
                    console.log(this.contentData);
                }.bind(this))
                .otherwise(function () {
                    console.log("Query did not return valid result");
                });
        },

        createTest: function () {
            var version = this.currentVersion.contentLink.split('_');
            var me = this;
            this.store.put({
                testDescription: this.testDescription,
                testContentId: this.contentData.contentGuid,
                publishedVersion: version[0],
                variantVersion: version[1],
                testDuration: this.testDuration,
                participationPercent: this.participationPercent,
                conversionPage: this.conversionPage,
                testTitle: this.testTitle,
                startDate: this.startDate
            }).then(function () {
                me.topic.publish("/epi/shell/action/changeview/back");
            }).otherwise(function () {
                console.log("Error occured while creating Marketing Test - Unable to create test");
            });
        }
    });

});