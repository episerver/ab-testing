define([
    // General application modules
    'dojo/_base/declare',
    // Parent class
    'epi-cms/component/command/_GlobalToolbarCommandProvider',
    // Other classes
    'marketing-testing/command/AddTest',
    'marketing-testing/command/CancelTest'
], function (declare, _GlobalToolbarCommandProvider, AddTest, CancelTest) {

    return declare([_GlobalToolbarCommandProvider], {

        constructor: function () {
            this.add('commands', new AddTest());
            this.add('commands', new CancelTest());
        }
    });
});