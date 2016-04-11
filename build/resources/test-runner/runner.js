var path = require("path");
var url = require("url");
var dnx = require("./lib/dnx");
var runner = require("./lib/test-runner");

var program = require("commander");

program.version("1.0.0")
    .option("-s, --sitePath [path]", "Path to the site to start")
    .option("-b, --buildAgentId [buildAgentId]", "Build Agent Id", 0)
    .option("-p, --port [port]", "Webserver default port", 8080)
    .option("-mf, --mochaTestFile [filename]", "Mocha test html file")
    .option("-df, --dojoTestFile [filename]", "Dojo test html file")
    .option("-r, --reporter [reporter]", "Mocha reporter to use, default is tap", "spec")
    .parse(process.argv);

var port = program.port ? parseInt(program.port, 10) : 8080;

//If we have a buildAgentId we should add that to the portnumber
if (program.buildAgentId) {
    port = port + parseInt(program.buildAgentId, 10);
}


// The code in mocha-phantomjs is not loading the 3rd-party reportes using require
// so we need to specify the path to the reporter
// see: https://github.com/nathanboktae/mocha-phantomjs/issues/135
var reporter = program.reporter;
if (reporter === "teamcity") {
    reporter = __dirname + "/node_modules/mocha-teamcity-reporter/lib/teamcity.js";
}

var localUri = "http://localhost:" + port;

var configurations = [{
    title: "Mocha Unit Tests",
    enabled: !!program.mochaTestFile,
    runner: "mocha-phantomjs",
    args: ["-R", reporter, "-t", 60000, url.resolve(localUri, program.mochaTestFile || ""), "-C"]
}, {
    title: "Dojo Unit Tests",
    enabled: !!program.dojoTestFile,
    runner: "phantomjs",
    args: [path.join(__dirname, "lib/run-dojo-tests.js"), url.resolve(localUri, program.dojoTestFile || "")]
}
];

var dnx = new dnx(program.sitePath, port);
dnx.start();

//start timer that will kill all running process after 5 minutes if it hasn't exited
var tearDownTimeout = setTimeout(function () {
    runner.kill();
    process.exit(1);
}, 300000);

runner.run(configurations)
    .catch(function (error) {
        console.error(error);

        runner.emit("error", error);
    })
    .finally(function () {
        dnx.kill();
        clearTimeout(tearDownTimeout);

        runner.emit("end");
    });
