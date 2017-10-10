var page = require('webpage').create(),
system = require('system'),
pageLink = system.args[1],
pageImage = system.args[2],
user = system.args[3],
pass = system.args[4];

page.viewportSize = { width: 1024, height: 768 };

page.open(pageLink, function () { });

page.onLoadFinished = function () {
    setTimeout(function () {
        page.evaluate(function () {
            document.getElementById("LoginControl_UserName").value = user;
            document.getElementById("LoginControl_Password").value = pass;
            document.getElementById("LoginControl_Button1").click();
        });
        setTimeout(function () {
            page.render(pageImage);
            phantom.exit();

        }, 1000);
    }, 1000);
};
