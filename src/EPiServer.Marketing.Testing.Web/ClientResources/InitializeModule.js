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

            //define our stores
            var commandRegistry = dependency.resolve("epi.globalcommandregistry"),
            abTestingStorePath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "ABTestStore" });
            abArchivesPath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "ABArchivedTestStore" });
            kpiStorePath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "KpiStore" });

            //create our store
            //preventCache:true required to get around IE's tendency to cache 'Get' calls.
            var abTestStore = new JsonRest({ target: abTestingStorePath, preventCache: true });
            var abArchives = new JsonRest({ target: abArchivesPath, preventCache: true });
            var kpiStore = new JsonRest({ target: kpiStorePath, preventCache: true });

            //add our store to the registry to be consumed by the UI
            registry.add("marketing.abtesting", abTestStore);
            registry.add("marketing.abarchives", abArchives);
            registry.add("marketing.kpistore", kpiStore);

            // config store for default values
            abTestingConfigStorePath = routes.getRestPath({ moduleArea: "EPiServer.Marketing.Testing", storeName: "ABTestConfigStore" });
            //create our store
            var abTestConfigStore = new JsonRest({ target: abTestingConfigStorePath });
            //add our store to the registry to be consumed by the UI
            registry.add("marketing.abtestingconfig", abTestConfigStore);

            editNotifications.add(TestNotification);

            commandRegistry.registerProvider('epi.cms.publishmenu', new AddTestCommandProvider());

            // Re-direct to a/b test context
            var hashWrapper = dependency.resolve("epi.shell.HashWrapper");
            var contextService = this.resolveDependency("epi.shell.ContextService");
            contextService.registerRoute("epi.marketing.testing", function (context, callerData) {
                hashWrapper.onContextChange(context, callerData);
            }.bind(this));
        }
    });
});