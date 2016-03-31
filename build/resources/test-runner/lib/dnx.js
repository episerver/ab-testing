"use strict";

const spawn = require("child_process").spawn

function dnx(sitePath, port) {
    this.sitePath = sitePath;
    this.port = port;
}

dnx.prototype = {
    constructor: dnx,
    start: function () {
        console.log("Starting dnx on port:", this.port, " for path:", this.sitePath);

        let promise = new Promise((resolve, reject) => {
            this._dnx = spawn("dnx", [
                "web-test",
                "--server.urls=http://*:" + this.port
            ], { cwd: this.sitePath });

            this._dnx.stdout.once('data', ()=> {
                this._dnx.stdout.end()
                resolve();
            });
            this._dnx.std
        });

        return promise;
    },

    kill: function () {
        console.log("Stopping dnx");

        if (this._dnx) {
            this._dnx.kill();
        }
    }
};

module.exports = dnx;
