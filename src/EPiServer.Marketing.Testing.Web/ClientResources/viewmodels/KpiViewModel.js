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

            getKpiByIndex: function (index) {
                return this.availableKpis[index];
            },

            createKpi: function (caller, kpiStore) {
                var me = this;
                this.kpistore = kpiStore || dependency.resolve("epi.storeregistry").get("marketing.kpistore");
                this.kpistore.put({
                    id: "KpiFormData",
                    entity: caller.kpiFormData
                })
                    .then(function (ret) {
                        if (ret.status) {
                            caller.createTest(ret);
                        } else {
                            if (ret.errors) {
                                caller.setKpiError(ret.errors)
                            } else {
                                caller.setKpiError(ret.message)
                            }
                        }
                    })
                    .otherwise(function (ret) {
                        caller.setKpiError(ret.response.xhr.statusText);
                    });
            },
        });

    });