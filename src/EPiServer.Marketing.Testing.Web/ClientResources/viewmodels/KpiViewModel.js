define([
        "dojo/_base/declare",
        "epi/dependency",
        "dojo/Stateful"
],
    function (
        declare,
        dependency,
        stateful
    ) {
        return declare([stateful],
        {
            availableKpis: null,

            constructor: function (kpiStore) {
                var me = this;
                this.kpistore = kpiStore || dependency.resolve("epi.storeregistry").get("marketing.kpistore");
                this.kpistore.get()
                .then(function (retKpis) {
                    me._changeAttrValue("availableKpis", retKpis);
                });
            },

            refreshKpis: function (kpiStore) {
                var me = this;
                this.kpistore = kpiStore || dependency.resolve("epi.storeregistry").get("marketing.kpistore");
                this.kpistore.get()
                .then(function (retKpis) {
                    me._changeAttrValue("availableKpis", retKpis);
                });
            },

            getAvailableKpis: function () {
                return this.availableKpis;
            },

            getKpiByIndex(index) {
                return this.availableKpis[index];
            },

            createKpi(caller, kpiStore) {
                var me = this;
                this.kpistore = kpiStore || dependency.resolve("epi.storeregistry").get("marketing.kpistore");
                this.kpistore.put({
                    id: "KpiFormData",
                    entity: caller.kpiFormData
                })
                    .then(function (ret) {
                        caller.createTest(ret);
                    })
                    .otherwise(function (ret) {
                        caller.setKpiError(ret);
                    });
            },
        });

    });