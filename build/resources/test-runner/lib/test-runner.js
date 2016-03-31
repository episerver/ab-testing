var async = require("async");
var path = require("path");
var findExecutable = require("./findModule").findExecutable;
var Promise = require("dojo/Promise"),
    Deferred = Promise.Deferred;
var spawn = require("child_process").spawn;

const EventEmitter = require('events');
const util = require('util');

var processList = [],
    currentExecution;


function runTests(testParams) {
    console.log("\nRunning " + testParams.title + "\n====================================================");

    var testRunner = findExecutable(testParams.runner);
    if (!testRunner) {
        var error = "Could not find " + testParams.runner + " executable";
        console.error(error);
        return Promise.reject(error);
    }

    var before = testParams.before ? testParams.before() : Promise.resolve();

    var promise = before.then(function () {
        var deferred = new Deferred();
        var phantom = spawn(testRunner, testParams.args, testParams.options);
        processList.push(phantom);

        phantom.stdin.setEncoding = 'utf-8';
        phantom.stdout.pipe(process.stdout);
        phantom.stderr.pipe(process.stdout);
        phantom.on("exit", deferred.resolve);
        return deferred.promise;
    });

    promise.catch(function (err) {
        console.error(err);
    }).finally(function () {
        testParams.after && testParams.after();
    });

    return promise;
}

function createRunner(configuration) {
    return function (done) {
        runTests(configuration)
            .then(function (code) {
                done(null, code === 0);
            }).catch(function () {
                done(null, false);
            });
    };
}

function TestRunner() {
    EventEmitter.call(this);
}
util.inherits(TestRunner, EventEmitter);

TestRunner.prototype = {
    constructor: TestRunner,
    run: function (configurations) {
        if (currentExecution) {
            return currentExecution;
        }

        currentExecution = new Deferred();
        var promise = currentExecution.promise;

        var runners = configurations
            .filter(function (suite) {
                return suite.enabled || suite.enabled === undefined;
            })
            .map(createRunner);

        async.series(runners, function (err, results) {
            currentExecution.resolve(results.every(function (r) {
                return r === true;
            }));
            currentExecution = null;
        });

        return promise;
    },

    kill: function () {
        processList.forEach(function (p) {
            p.kill();
        });
        if (currentExecution) {
            currentExecution.reject();
            currentExecution = null;
        }
    }
};

module.exports = new TestRunner();
