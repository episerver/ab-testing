'use strict';

const gulp = require('gulp'),
      program = require('commander');

program
    .option('-r, --reporter [reporter]', 'Mocha reporter to use, default is tap', 'spec')
    .option('-p, --port [port]', 'Webserver default port', 8080)
    .parse(process.argv);

// Import all build tasks
require('./build/tasks/extract')();
require('./build/tasks/unit-tests')(program.reporter, program.port);

gulp.task('test', ['run-js-tests']);
