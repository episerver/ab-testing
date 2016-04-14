define(['marketing-testing/command/AddTestCommandProvider',
        'marketing-testing/command/AddTest'
       ],
    function (AddTestCommandProvider, AddTest) {
        describe("AddTestCommandProvider", function () {
            it("adds the AddTest command to the global toolbar command collection on construction", function () {
                var aCommandProvider = new AddTestCommandProvider();

                expect(aCommandProvider.commands).to.not.be.undefined;
                expect(aCommandProvider.commands[0]).to.be.instanceof(AddTest);
            });
        });
    });