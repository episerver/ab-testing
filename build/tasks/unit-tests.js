'use strict';

const gulp = require('gulp'),
      path = require('path'),
      testRunner = require('test-runner'),
      url = require('url');

let tasks = (reporter, port)=> {

    // The code in mocha-phantomjs is not loading the 3rd-party reportes using require
    // so we need to specify the path to the reporter
    // see: https://github.com/nathanboktae/mocha-phantomjs/issues/135
    if (reporter === 'teamcity') {
        reporter = path.resolve(__dirname, '../../node_modules/mocha-teamcity-reporter/lib/teamcity.js');
    }

    function start(sitePath, port, done) {
        port = 8080 + (port % 5000);
        const localUri = 'http://localhost:' + port;

        const dnx  = new testRunner.dnx(sitePath, port);
        dnx.start().then(()=> {
            const configurations = [
                {
                    title: 'Mocha Unit Tests',
                    enabled: true,
                    runner: 'mocha-phantomjs',
                    args: ['-R', reporter, '-t', 60000, localUri]
                }];

            var tearDownTimeout = setTimeout(function () {
                testRunner.runner.kill();
            }, 300000);

            testRunner.runner.run(configurations)
                .then((success) => {
                    dnx.kill();
                    if (!success) {
                        done('Unit Tests failed')
                    } else {
                        done();
                    }
                })
                .catch(function (error) {
                    console.error(error);
                    dnx.kill();
                    done(error);
                })
                .finally(function () {
                    clearTimeout(tearDownTimeout);
                });
        });
    };

    gulp.task('run-js-tests', ['extract-js-sources'], (done) => {
        start(path.join(__dirname, '../../test/EPiServer.Marketing.Testing.Web.Test'), port, done);
    });
};

module.exports = tasks;
