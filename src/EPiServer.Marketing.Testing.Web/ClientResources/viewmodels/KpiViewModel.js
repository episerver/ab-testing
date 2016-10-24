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
            }
        });
    });