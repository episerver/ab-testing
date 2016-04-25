define(["marketing-testing/command/CancelTest"],
    function (CancelTestCommand) {
        describe("CancelTest command", function () {
            it("Is visible when there is a test setup on the current page");
            it("Is not visible unless there is a test setup on the current page");
            it("Calls to cancel the current test setup when executed");
        });
    });