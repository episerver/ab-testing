define([
   // General application modules
   'dojo/_base/declare', 'epi/dependency', 'epi/routes',
   // Parent class
   'epi/_Module',
   // Other classes
   'marketing-testing/command/AddTestCommandProvider',
   // For Store
   'epi/shell/store/JsonRest'
], function (declare, dependency, routes, _Module, AddTestCommandProvider, JsonRest) {

    return declare([_Module], {

        initialize: function () {
            // summary:
            //      Initializes the favorite module.
            // tags:
            //      public
            //get epi's store registry which we will add our own store to.
            var registry = this.resolveDependency("epi.storeregistry");

            //define our store
            var commandRegistry = dependency.resolve("epi.globalcommandregistry"),
                testingStorePath = routes.getRestPath({
                    moduleArea: "EPiServer.Multivariate",
                    storeName: "MarketingTestingStore"
                });

            //create our store
            var store = new JsonRest({ target: testingStorePath });

            //add our store to the registry to be consumed by the UI
            registry.add("marketing.testing", store);

            commandRegistry.registerProvider('epi.cms.publishmenu', new AddTestCommandProvider());
        }
    });
});