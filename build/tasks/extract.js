'use strict';

const decompress = require('gulp-decompress'),
      fs = require('fs'),
      gulp = require('gulp'),
      gutil = require('gulp-util'),
      path = require('path');

let tasks = () => {

    gulp.task('extract-js-sources', (done) => {
        const zipFile = path.resolve('build/resources/EPiServer.UI.Sources.zip'),
              destination = path.resolve('test/DtkBinaries/dojo-release-1.8.9-src');

        fs.stat(destination, (err, stat) => {
            if (stat && stat.isDirectory()) {
                gutil.log(zipFile, 'has already been decompressed to:', destination);
                return done();
            }

            gulp.src(zipFile)
                .pipe(decompress({}))
                .pipe(gulp.dest(destination))
                .on('end', done);
        });
    });
};

module.exports = tasks;
