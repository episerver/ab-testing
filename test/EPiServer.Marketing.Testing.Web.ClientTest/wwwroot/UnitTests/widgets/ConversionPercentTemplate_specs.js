define([
        'marketing-testing/widgets/ConversionPercentTemplate',
        'epi/_Module',
        'dojo/dom-class'
],
    function (ConversionPercentTemplate, Module, domClass) {
        describe("Conversion Percent Template (widget)",
            function () {
                var mockStringResources = {
                    percentagetemplate: {
                        views: "Views",
                        conversion_rate: "Conversion Rate"
                    }
                };

                it("Correctly initializes a default Conversion Percent Template",
                    function () {
                        var testWidget = new ConversionPercentTemplate({
                            resources: mockStringResources,
                            conversionPercent: 10,
                            views: 5,
                       });

                        expect(testWidget.conversionPercent).to.equal(10);
                        expect(testWidget.views).to.equal(5);
                        expect(testWidget.percentageNode.innerHTML).to.equal("10%");
                        expect(testWidget.viewsNode.innerHTML).to.equal("5");
                    });

                it("Correctly adjusts styles when isLeader is true",
                    function () {
                        var testWidget = new ConversionPercentTemplate({
                            resources: mockStringResources,
                            conversionPercent: 25,
                            views: 10,
                            isLeader: true
                        });

                        expect(domClass.contains(testWidget.viewsNode, "epi-kpiSummary-conversionRate-leader")).to.equal(true);
                        expect(domClass.contains(testWidget.percentageNode, "epi-kpiSummary-conversionRate-leader")).to.equal(true);
                    });

                it("Correctly adjusts styles when isLeader is false",
                    function () {
                        var testWidget = new ConversionPercentTemplate({
                            resources: mockStringResources,
                            conversionPercent: 25,
                            views: 10,
                            isLeader: false
                        });

                        expect(domClass.contains(testWidget.viewsNode, "epi-kpiSummary-conversionRate-default")).to.equal(true);
                        expect(domClass.contains(testWidget.percentageNode, "epi-kpiSummary-conversionRate-default")).to.equal(true);
                    });
            });
    });