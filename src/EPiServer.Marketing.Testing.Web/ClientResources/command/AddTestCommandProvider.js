define([
    // General application modules
    'dojo/_base/declare',
    // Parent class
    'epi-cms/component/command/_GlobalToolbarCommandProvider',
    // Other classes
    'marketing-testing/command/AddTest'
], function (declare, _GlobalToolbarCommandProvider, AddTest) {

    return declare([_GlobalToolbarCommandProvider], {

        constructor: function () {
            this.add('commands', new AddTest());
            this.add('commands', new CancelTest());
        }
    });
});