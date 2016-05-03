define([
     'dojo/_base/declare',
    'epi/dependency',
    'dojo/Stateful',
    'dojo/topic'
], function (
    declare,
    dependency,
    Stateful,
    topic
) {
    return declare([Stateful], {

        // testId: Number
        //      Test ID
        testId: null,

        // testState: Number
        //      Test state        
        testState: 0,

        // testTitle: String
        //      Test title
        testTitle: null,

        // testDescription: String
        //      Test description
        testDescription: null,

        // variation: String
        //      Variation
        variation: null,

        // contentId: String
        //      Content ID of the content under test
        contentId: null,

        // conversionPage: String
        //      Page, which when loaded, triggers a conversion from a page under test.
        conversionPage: null,

        // participationPercent: Number
        //      Percentage of visitors to include in the test.
        participationPercent: null,

        //duration, in days, the test should run.
        testDuration: null,

        //start date (currently set to "now" when they hit the start test button
        startDate: null,
        
        _test: null,

        postscript: function () {
            this.inherited(arguments);
            if (arguments.test) {
                this.testId = test.Id;
            }
        }
    });

});