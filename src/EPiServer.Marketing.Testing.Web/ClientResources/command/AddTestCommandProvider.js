define([
    // General application modules
    'dojo/_base/declare',
    // Parent class
    'epi/shell/command/_CommandProviderMixin',
    // Other classes
    'marketing-testing/command/AddTest',
    'marketing-testing/command/CancelTest'
], function (declare, _CommandProviderMixin, AddTest, CancelTest) {

    return declare([_CommandProviderMixin], {

        constructor: function () {
            this.add('commands', new AddTest());
            this.add('commands', new CancelTest());
        }
    });
});