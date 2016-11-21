define(['marketing-testing/viewmodels/KpiViewModel',
        'epi/_Module'],
    function (KpiViewModel, Module) {
        describe("KpiViewModel", function () {
            var mockAvailableKpis = ["MockKpi1", "MockKpi2"];

            it("populates the list of available kpis when kpiViewModel is created", function () {
                    
                var mockKpiModel = new KpiViewModel({
                        get: function () {
                            return this;
                        },
                        then: function (successFunction) {
                            successFunction(mockAvailableKpis);
                            return this;
                        },
                        otherwise: function (otherwiseFunction) {
                        }
                });

                expect(mockKpiModel.availableKpis[0]).to.equal(mockAvailableKpis[0]);
                expect(mockKpiModel.availableKpis[1]).to.equal(mockAvailableKpis[1]);
            });

            it("returns the correct list of kpis when getAvailableKpis is called", function () {
                var mockKpiModel = new KpiViewModel({
                    get: function () {
                        return this;
                    },
                    then: function (successFunction) {
                        successFunction(mockAvailableKpis);
                        return this;
                    },
                    otherwise: function (otherwiseFunction) {
                    }
                });

                var returnedKpis = mockKpiModel.getAvailableKpis();

                expect(returnedKpis[0]).to.equal(mockAvailableKpis[0]);
                expect(returnedKpis[1]).to.equal(mockAvailableKpis[1]);
            });

            it("returns the correct kpis when getKpiByIndex(index) is called", function () {
                var mockKpiModel = new KpiViewModel({
                    get: function () {
                        return this;
                    },
                    then: function (successFunction) {
                        successFunction(mockAvailableKpis);
                        return this;
                    },
                    otherwise: function (otherwiseFunction) {
                    }
                });

                expect(mockKpiModel.getKpiByIndex(0)).to.equal(mockAvailableKpis[0]);
                expect(mockKpiModel.getKpiByIndex(1)).to.equal(mockAvailableKpis[1]);
            });
        });
    });