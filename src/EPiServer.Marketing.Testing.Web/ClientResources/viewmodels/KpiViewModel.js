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
            availableKpi: null,

            constructor: function () {
                var me = this;
                this.kpistore = dependency.resolve("epi.storeregistry").get("marketing.kpistore");
                this.kpistore.get()
                .then(function (markup) {
                    me._changeAttrValue("availableKpi", markup);
                });
            },

            getAvailableKpis: function () {
                return this.availableKpi;
            },

            getKpiByIndex(index) {
                return this.availableKpi[index];
            },

            createKpi(caller) {
                var me = this;
                this.kpistore = dependency.resolve("epi.storeregistry").get("marketing.kpistore");
                this.kpistore.put({
                    id: "KpiFormData",
                    entity: caller.kpiFormData
                })
                    .then(function (ret) {
                        caller._clearConversionErrors();
                        caller.model.kpiId = ret;
                        if (caller._isValidFormData()) {
                            caller.model.createTest();
                            caller._clearConversionErrors();
                            caller._setKpiSelectList(caller.kpiModel.availableKpi);
                        } else {
                            caller.startButtonClickCounter = 0;
                        }
                    })
                    .otherwise(function (ret) {
                        caller._setError(ret.response.xhr.statusText, caller.kpiErrorTextNode, caller.kpiErrorIconNode);
                        caller.startButtonClickCounter = 0;
                    });
            },
        });

    });