define([
   // General application modules
   'dojo/_base/declare', 'epi/dependency', 'epi/routes',
   // Parent class
   'epi/_Module',
   // Other classes
   "epi-cms/plugin-area/edit-notifications",

   'marketing-testing/TestNotification',
   'marketing-testing/command/AddTestCommandProvider',
   // For Store
   'epi/shell/store/JsonRest'
], function (declare, dependency, routes, _Module, editNotifications, TestNotification, AddTestCommandProvider, JsonRest) {

    return declare([_Module], {

        initialize: function () {
            // summary:
            //      Initializes the AB Testing module.
            // tags:
            //      public
            //get epi's store registry which we will add our own store to.
            var registry = this.resolveDependency("epi.storeregistry");

            //define our store
            var commandRegistry = dependency.resolve("epi.globalcommandregistry"),
                testingStorePath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "MarketingTestingStore" }),
                contentTestingStorePath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "MarketingContentTestingStore" }),
                testingResultStorePath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "MarketingTestingResultStore" });

            //create our store
            var store = new JsonRest({ target: testingStorePath });
            var contentTestStore = new JsonRest({ target: contentTestingStorePath });
            var resultTestStore = new JsonRest({ target: testingResultStorePath });
            //add our store to the registry to be consumed by the UI
            registry.add("marketing.testing", store);
            registry.add("marketing.contentTesting", contentTestStore);
            registry.add("marketing.testingResult", resultTestStore);

            
            editNotifications.add(TestNotification);

            commandRegistry.registerProvider('epi.cms.publishmenu', new AddTestCommandProvider());
        }
    });
});